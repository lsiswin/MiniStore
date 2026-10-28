using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Data;
using MiniStore.Api.DTOs;
using MiniStore.Api.Entities;
using MiniStore.Api.Services;

namespace MiniStore.Api.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "用户名和密码不能为空" });
        }

        // Login check needs IgnoreQueryFilters to find the user across all tenants
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }

        var store = await _context.Stores
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == user.TenantId);

        var token = _jwtService.GenerateToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            User = new CurrentUserDto
            {
                Id = user.Id,
                TenantId = user.TenantId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Role = user.Role,
                StoreName = store?.Name ?? "未命名小店",
                StoreSlug = store?.Slug ?? string.Empty
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return Unauthorized();
        }

        var store = await _context.Stores.FirstOrDefaultAsync(s => s.TenantId == user.TenantId);

        return Ok(new CurrentUserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role,
            StoreName = store?.Name ?? "未命名小店",
            StoreSlug = store?.Slug ?? string.Empty
        });
    }
}
