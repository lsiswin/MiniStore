using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;
using MiniStore.Api.Entities;

namespace MiniStore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/services")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicesController(AppDbContext context)
    {
        _context = context;
    }

    #region Categories

    [HttpGet("categories")]
    public async Task<ActionResult<List<ServiceCategoryDto>>> GetCategories()
    {
        var categories = await _context.ServiceCategories
            .OrderBy(c => c.SortOrder)
            .Select(c => new ServiceCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SortOrder = c.SortOrder,
                ServicesCount = _context.Services.Count(s => s.CategoryId == c.Id)
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<ServiceCategoryDto>> CreateCategory([FromBody] UpsertCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "分类名称不能为空" });
        }

        var cat = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceCategories.Add(cat);
        await _context.SaveChangesAsync();

        return Ok(new ServiceCategoryDto
        {
            Id = cat.Id,
            Name = cat.Name,
            SortOrder = cat.SortOrder,
            ServicesCount = 0
        });
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpsertCategoryRequest request)
    {
        var cat = await _context.ServiceCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null)
        {
            return NotFound(new { message = "未找到该分类" });
        }

        cat.Name = request.Name.Trim();
        cat.SortOrder = request.SortOrder;
        await _context.SaveChangesAsync();

        return Ok(new { message = "分类更新成功" });
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var cat = await _context.ServiceCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null)
        {
            return NotFound(new { message = "未找到该分类" });
        }

        var hasServices = await _context.Services.AnyAsync(s => s.CategoryId == id);
        if (hasServices)
        {
            return BadRequest(new { message = "该分类下仍有服务项目，无法直接删除" });
        }

        _context.ServiceCategories.Remove(cat);
        await _context.SaveChangesAsync();

        return Ok(new { message = "分类已删除" });
    }

    #endregion

    #region Service Items

    [HttpGet("items")]
    public async Task<ActionResult<List<ServiceItemDto>>> GetServiceItems()
    {
        var categories = await _context.ServiceCategories.ToDictionaryAsync(c => c.Id, c => c.Name);

        var items = await _context.Services
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        var dtos = items.Select(s => new ServiceItemDto
        {
            Id = s.Id,
            CategoryId = s.CategoryId,
            CategoryName = categories.TryGetValue(s.CategoryId, out var catName) ? catName : "未分类",
            Name = s.Name,
            Price = s.Price,
            DurationMinutes = s.DurationMinutes,
            ImageUrl = s.ImageUrl,
            Description = s.Description,
            Enabled = s.Enabled,
            SortOrder = s.SortOrder
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost("items")]
    public async Task<ActionResult<ServiceItemDto>> CreateServiceItem([FromBody] UpsertServiceItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "服务名称不能为空" });
        }

        var item = new ServiceItem
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            Enabled = request.Enabled,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Services.Add(item);
        await _context.SaveChangesAsync();

        var cat = await _context.ServiceCategories.FirstOrDefaultAsync(c => c.Id == item.CategoryId);

        return Ok(new ServiceItemDto
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            CategoryName = cat?.Name ?? "未分类",
            Name = item.Name,
            Price = item.Price,
            DurationMinutes = item.DurationMinutes,
            ImageUrl = item.ImageUrl,
            Description = item.Description,
            Enabled = item.Enabled,
            SortOrder = item.SortOrder
        });
    }

    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateServiceItem(Guid id, [FromBody] UpsertServiceItemRequest request)
    {
        var item = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null)
        {
            return NotFound(new { message = "未找到该服务项目" });
        }

        item.CategoryId = request.CategoryId;
        item.Name = request.Name.Trim();
        item.Price = request.Price;
        item.DurationMinutes = request.DurationMinutes;
        item.ImageUrl = request.ImageUrl;
        item.Description = request.Description;
        item.Enabled = request.Enabled;
        item.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync();
        return Ok(new { message = "服务项目已更新" });
    }

    [HttpPatch("items/{id}/toggle")]
    public async Task<IActionResult> ToggleServiceItem(Guid id)
    {
        var item = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null)
        {
            return NotFound(new { message = "未找到该服务项目" });
        }

        item.Enabled = !item.Enabled;
        await _context.SaveChangesAsync();

        return Ok(new { message = "状态已切换", enabled = item.Enabled });
    }

    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeleteServiceItem(Guid id)
    {
        var item = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null)
        {
            return NotFound(new { message = "未找到该服务项目" });
        }

        _context.Services.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new { message = "服务项目已删除" });
    }

    #endregion
}
