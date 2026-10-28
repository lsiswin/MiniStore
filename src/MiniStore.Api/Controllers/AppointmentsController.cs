using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;

namespace MiniStore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentListItemDto>>> GetAppointments(
        [FromQuery] string? date = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Appointments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var filterDate))
        {
            var nextDate = filterDate.Date.AddDays(1);
            query = query.Where(a => a.AppointmentAt >= filterDate.Date && a.AppointmentAt < nextDate);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status.ToLower() == status.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(a => a.CustomerName.Contains(s) || a.CustomerPhone.Contains(s) || a.ServiceName.Contains(s));
        }

        var list = await query
            .OrderByDescending(a => a.AppointmentAt)
            .Select(a => new AppointmentListItemDto
            {
                Id = a.Id,
                AppointmentAt = a.AppointmentAt,
                Status = a.Status,
                CustomerName = a.CustomerName,
                CustomerPhone = a.CustomerPhone,
                ServiceName = a.ServiceName,
                Price = a.Price,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        var validStatuses = new[] { "pending", "confirmed", "completed", "cancelled" };
        if (!validStatuses.Contains(request.Status.ToLower()))
        {
            return BadRequest(new { message = "无效的预约状态" });
        }

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appointment == null)
        {
            return NotFound(new { message = "未找到该预约记录" });
        }

        appointment.Status = request.Status.ToLower();
        await _context.SaveChangesAsync();

        return Ok(new { message = "状态已更新", status = appointment.Status });
    }
}
