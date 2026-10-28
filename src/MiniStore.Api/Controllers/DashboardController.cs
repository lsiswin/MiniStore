using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;

namespace MiniStore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Global query filter ensures we only query current tenant's data!
        var todayAppointments = await _context.Appointments
            .Where(a => a.AppointmentAt >= today && a.AppointmentAt < tomorrow)
            .ToListAsync();

        var pendingCount = await _context.Appointments
            .Where(a => a.Status == "pending")
            .CountAsync();

        var totalCustomers = await _context.Customers
            .CountAsync();

        var recentAppointments = await _context.Appointments
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
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

        var result = new DashboardSummaryDto
        {
            TodayAppointmentsCount = todayAppointments.Count,
            PendingAppointmentsCount = pendingCount,
            TotalCustomersCount = totalCustomers,
            TodayEstimatedRevenue = todayAppointments
                .Where(a => a.Status != "cancelled")
                .Sum(a => a.Price),
            RecentAppointments = recentAppointments
        };

        return Ok(result);
    }
}
