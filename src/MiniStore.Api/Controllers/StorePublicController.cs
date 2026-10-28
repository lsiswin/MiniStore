using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;
using MiniStore.Api.Entities;

namespace MiniStore.Api.Controllers;

[ApiController]
[Route("api/s")]
public class StorePublicController : ControllerBase
{
    private readonly AppDbContext _context;

    public StorePublicController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<StorePublicDto>> GetStoreBySlug(string slug)
    {
        // Public API: IgnoreQueryFilters because we're querying by public store slug
        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug.ToLower());

        if (store == null)
        {
            return NotFound(new { message = $"未找到标识为 '{slug}' 的门店" });
        }

        var categories = await _context.ServiceCategories
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == store.TenantId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var services = await _context.Services
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == store.TenantId && s.Enabled)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        var result = new StorePublicDto
        {
            StoreId = store.Id,
            Name = store.Name,
            Slug = store.Slug,
            Logo = store.Logo,
            Banner = store.Banner,
            Phone = store.Phone,
            Address = store.Address,
            Hours = store.Hours,
            Announcement = store.Announcement,
            Categories = categories.Select(c => new CategoryPublicDto
            {
                Id = c.Id,
                Name = c.Name,
                SortOrder = c.SortOrder,
                Services = services
                    .Where(s => s.CategoryId == c.Id)
                    .Select(s => new ServicePublicDto
                    {
                        Id = s.Id,
                        CategoryId = s.CategoryId,
                        Name = s.Name,
                        Price = s.Price,
                        DurationMinutes = s.DurationMinutes,
                        ImageUrl = s.ImageUrl,
                        Description = s.Description
                    }).ToList()
            }).ToList()
        };

        return Ok(result);
    }

    [HttpPost("{slug}/book")]
    public async Task<ActionResult<AppointmentResultDto>> BookAppointment(string slug, [FromBody] BookAppointmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            return BadRequest(new { message = "请填写顾客姓名与联系手机" });
        }

        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug.ToLower());

        if (store == null)
        {
            return NotFound(new { message = "门店不存在" });
        }

        var service = await _context.Services
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.TenantId == store.TenantId);

        if (service == null || !service.Enabled)
        {
            return BadRequest(new { message = "所选服务项目不存在或已下架" });
        }

        // Check if customer exists in this tenant, if not, create customer
        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TenantId == store.TenantId && c.Phone == request.CustomerPhone.Trim());

        if (customer == null)
        {
            customer = new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = store.TenantId,
                Name = request.CustomerName.Trim(),
                Phone = request.CustomerPhone.Trim(),
                VisitCount = 1,
                LastVisitAt = DateTime.UtcNow,
                Notes = "在线预约新客",
                CreatedAt = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
        }
        else
        {
            customer.VisitCount += 1;
            customer.LastVisitAt = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                customer.Name = request.CustomerName.Trim();
            }
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = store.TenantId,
            StoreId = store.Id,
            ServiceId = service.Id,
            CustomerId = customer.Id,
            CustomerName = request.CustomerName.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            ServiceName = service.Name,
            Price = service.Price,
            AppointmentAt = request.AppointmentAt,
            Status = "pending",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return Ok(new AppointmentResultDto
        {
            AppointmentId = appointment.Id,
            StoreName = store.Name,
            ServiceName = service.Name,
            Price = service.Price,
            AppointmentAt = appointment.AppointmentAt,
            CustomerName = appointment.CustomerName,
            CustomerPhone = appointment.CustomerPhone,
            Status = appointment.Status,
            Message = "预约成功！商家已收到通知，请按时到店体验。"
        });
    }

    [HttpGet("{slug}/customer/profile")]
    public async Task<ActionResult<CustomerProfileDto>> GetCustomerProfile(string slug, [FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new { message = "手机号不能为空" });
        }

        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug.ToLower());

        if (store == null) return NotFound(new { message = "门店不存在" });

        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TenantId == store.TenantId && c.Phone == phone.Trim());

        if (customer == null)
        {
            // If customer hasn't booked yet, return empty placeholder
            return Ok(new CustomerProfileDto
            {
                Name = "新顾客",
                Phone = phone.Trim(),
                VisitCount = 0
            });
        }

        var cards = await _context.CustomerCards
            .IgnoreQueryFilters()
            .Where(cc => cc.TenantId == store.TenantId && cc.CustomerId == customer.Id)
            .OrderByDescending(cc => cc.CreatedAt)
            .Select(cc => new CustomerCardItemDto
            {
                Id = cc.Id,
                CardName = cc.CardName,
                RemainingCount = cc.RemainingCount,
                ExpireAt = cc.ExpireAt
            })
            .ToListAsync();

        return Ok(new CustomerProfileDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            VisitCount = customer.VisitCount,
            LastVisitAt = customer.LastVisitAt,
            Notes = customer.Notes,
            Cards = cards
        });
    }

    [HttpPut("{slug}/customer/profile")]
    public async Task<IActionResult> UpdateCustomerProfile(string slug, [FromQuery] string phone, [FromBody] UpdateCustomerProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "手机号和姓名不能为空" });
        }

        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug.ToLower());

        if (store == null) return NotFound(new { message = "门店不存在" });

        var customer = await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TenantId == store.TenantId && c.Phone == phone.Trim());

        if (customer == null)
        {
            customer = new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = store.TenantId,
                Name = request.Name.Trim(),
                Phone = phone.Trim(),
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
        }
        else
        {
            customer.Name = request.Name.Trim();
            customer.Notes = request.Notes;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "资料更新成功", name = customer.Name });
    }

    [HttpGet("{slug}/customer/appointments")]
    public async Task<ActionResult<List<CustomerAppointmentDto>>> GetCustomerAppointments(string slug, [FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new { message = "手机号不能为空" });
        }

        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug.ToLower());

        if (store == null) return NotFound(new { message = "门店不存在" });

        var list = await _context.Appointments
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == store.TenantId && a.CustomerPhone == phone.Trim())
            .OrderByDescending(a => a.AppointmentAt)
            .Select(a => new CustomerAppointmentDto
            {
                Id = a.Id,
                ServiceName = a.ServiceName,
                Price = a.Price,
                AppointmentAt = a.AppointmentAt,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPut("{slug}/customer/appointments/{id}/cancel")]
    public async Task<IActionResult> CancelCustomerAppointment(string slug, Guid id, [FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new { message = "手机号不能为空" });
        }

        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug.ToLower());

        if (store == null) return NotFound(new { message = "门店不存在" });

        var appointment = await _context.Appointments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == store.TenantId && a.CustomerPhone == phone.Trim());

        if (appointment == null)
        {
            return NotFound(new { message = "未找到该预约单" });
        }

        if (appointment.Status == "completed")
        {
            return BadRequest(new { message = "该预约已完成，无法取消" });
        }

        if (appointment.Status == "cancelled")
        {
            return BadRequest(new { message = "该预约已经是取消状态" });
        }

        appointment.Status = "cancelled";
        await _context.SaveChangesAsync();

        return Ok(new { message = "预约已成功取消", status = appointment.Status });
    }
}
