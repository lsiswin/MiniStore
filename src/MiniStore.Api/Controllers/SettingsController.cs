using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;

namespace MiniStore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/settings")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<StoreSettingsDto>> GetSettings()
    {
        var store = await _context.Stores.FirstOrDefaultAsync();
        if (store == null)
        {
            return NotFound(new { message = "未找到门店设置" });
        }

        return Ok(new StoreSettingsDto
        {
            Id = store.Id,
            Name = store.Name,
            Slug = store.Slug,
            Logo = store.Logo,
            Banner = store.Banner,
            Phone = store.Phone,
            Address = store.Address,
            Hours = store.Hours,
            Announcement = store.Announcement
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateStoreSettingsRequest request)
    {
        var store = await _context.Stores.FirstOrDefaultAsync();
        if (store == null)
        {
            return NotFound(new { message = "未找到门店设置" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "店名不能为空" });
        }

        store.Name = request.Name.Trim();
        store.Logo = request.Logo;
        store.Banner = request.Banner;
        store.Phone = request.Phone;
        store.Address = request.Address;
        store.Hours = request.Hours;
        store.Announcement = request.Announcement;

        await _context.SaveChangesAsync();
        return Ok(new { message = "门店设置已保存" });
    }
}
