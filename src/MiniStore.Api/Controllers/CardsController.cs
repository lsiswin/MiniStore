using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;
using MiniStore.Api.Entities;

namespace MiniStore.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/cards")]
public class CardsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CardsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("templates")]
    public async Task<ActionResult<List<MemberCardDto>>> GetTemplates()
    {
        var templates = await _context.MemberCards
            .Select(c => new MemberCardDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                Price = c.Price,
                TotalCount = c.TotalCount,
                ValidDays = c.ValidDays,
                Description = c.Description,
                ActiveCardsCount = _context.CustomerCards.Count(cc => cc.CardId == c.Id && cc.RemainingCount > 0)
            })
            .ToListAsync();

        return Ok(templates);
    }

    [HttpPost("templates")]
    public async Task<ActionResult<MemberCardDto>> CreateTemplate([FromBody] CreateMemberCardRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "卡名称不能为空" });
        }

        var card = new MemberCard
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = request.Type,
            Price = request.Price,
            TotalCount = request.TotalCount,
            ValidDays = request.ValidDays,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.MemberCards.Add(card);
        await _context.SaveChangesAsync();

        return Ok(new MemberCardDto
        {
            Id = card.Id,
            Name = card.Name,
            Type = card.Type,
            Price = card.Price,
            TotalCount = card.TotalCount,
            ValidDays = card.ValidDays,
            Description = card.Description,
            ActiveCardsCount = 0
        });
    }

    [HttpDelete("templates/{id}")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var card = await _context.MemberCards.FirstOrDefaultAsync(c => c.Id == id);
        if (card == null)
        {
            return NotFound(new { message = "未找到该卡模板" });
        }

        _context.MemberCards.Remove(card);
        await _context.SaveChangesAsync();

        return Ok(new { message = "卡模板已删除" });
    }

    [HttpGet("customer-cards")]
    public async Task<ActionResult<List<CustomerCardDto>>> GetCustomerCards()
    {
        var customers = await _context.Customers.ToDictionaryAsync(c => c.Id, c => (c.Name, c.Phone));

        var cards = await _context.CustomerCards
            .OrderByDescending(cc => cc.CreatedAt)
            .ToListAsync();

        var dtos = cards.Select(cc => new CustomerCardDto
        {
            Id = cc.Id,
            CustomerId = cc.CustomerId,
            CustomerName = customers.TryGetValue(cc.CustomerId, out var cInfo) ? cInfo.Name : "未知客户",
            CustomerPhone = customers.TryGetValue(cc.CustomerId, out var cInfo2) ? cInfo2.Phone : "",
            CardId = cc.CardId,
            CardName = cc.CardName,
            RemainingCount = cc.RemainingCount,
            ExpireAt = cc.ExpireAt,
            CreatedAt = cc.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost("bind")]
    public async Task<IActionResult> BindCustomerCard([FromBody] BindCustomerCardRequest request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
        if (customer == null)
        {
            return NotFound(new { message = "未找到该客户" });
        }

        var template = await _context.MemberCards.FirstOrDefaultAsync(c => c.Id == request.CardId);
        if (template == null)
        {
            return NotFound(new { message = "未找到该卡模板" });
        }

        var custCard = new CustomerCard
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CardId = template.Id,
            CardName = template.Name,
            RemainingCount = template.TotalCount,
            ExpireAt = DateTime.UtcNow.AddDays(template.ValidDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerCards.Add(custCard);
        await _context.SaveChangesAsync();

        return Ok(new { message = "开卡成功", cardId = custCard.Id });
    }

    [HttpPost("deduct")]
    public async Task<IActionResult> DeductCard([FromBody] DeductCardRequest request)
    {
        var custCard = await _context.CustomerCards.FirstOrDefaultAsync(cc => cc.Id == request.CustomerCardId);
        if (custCard == null)
        {
            return NotFound(new { message = "未找到该客户次卡" });
        }

        if (custCard.RemainingCount < request.DeductCount)
        {
            return BadRequest(new { message = $"次卡剩余次数不足，当前剩余 {custCard.RemainingCount} 次" });
        }

        custCard.RemainingCount -= request.DeductCount;

        var record = new CardRecord
        {
            Id = Guid.NewGuid(),
            CustomerCardId = custCard.Id,
            Change = -request.DeductCount,
            RemainingAfter = custCard.RemainingCount,
            Remark = string.IsNullOrWhiteSpace(request.Remark) ? $"到店核销 {request.DeductCount} 次" : request.Remark,
            CreatedAt = DateTime.UtcNow
        };

        _context.CardRecords.Add(record);

        // Update customer visit count and last visit
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == custCard.CustomerId);
        if (customer != null)
        {
            customer.VisitCount += 1;
            customer.LastVisitAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "核销成功",
            remainingCount = custCard.RemainingCount
        });
    }
}
