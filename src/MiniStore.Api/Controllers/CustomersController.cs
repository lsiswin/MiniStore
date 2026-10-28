using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;
using MiniStore.Api.Entities;

namespace MiniStore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerListItemDto>>> GetCustomers([FromQuery] string? search = null)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(c => c.Name.Contains(s) || c.Phone.Contains(s));
        }

        var list = await query
            .OrderByDescending(c => c.LastVisitAt ?? c.CreatedAt)
            .Select(c => new CustomerListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                VisitCount = c.VisitCount,
                LastVisitAt = c.LastVisitAt,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerListItemDto>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new { message = "客户姓名和手机号不能为空" });
        }

        var phone = request.Phone.Trim();
        var existing = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
        if (existing != null)
        {
            return Conflict(new { message = "该手机号的客户已存在" });
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Phone = phone,
            VisitCount = 0,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(new CustomerListItemDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            VisitCount = customer.VisitCount,
            LastVisitAt = customer.LastVisitAt,
            Notes = customer.Notes,
            CreatedAt = customer.CreatedAt
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerDetail(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null)
        {
            return NotFound(new { message = "未找到该客户" });
        }

        var cards = await _context.CustomerCards
            .Where(cc => cc.CustomerId == id)
            .ToListAsync();

        var appointments = await _context.Appointments
            .Where(a => a.CustomerId == id)
            .OrderByDescending(a => a.AppointmentAt)
            .Take(20)
            .ToListAsync();

        return Ok(new
        {
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.VisitCount,
            customer.LastVisitAt,
            customer.Notes,
            customer.CreatedAt,
            Cards = cards,
            Appointments = appointments
        });
    }
}
