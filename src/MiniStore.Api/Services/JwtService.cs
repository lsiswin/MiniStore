using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MiniStore.Api.Entities;

namespace MiniStore.Api.Services;

public interface IJwtService
{
    string GenerateToken(MerchantUser user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(MerchantUser user)
    {
        var secret = _configuration["Jwt:Secret"] ?? "MiniStore_Super_Secret_Key_For_Jwt_Auth_2026_Key_Must_Be_Long";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("username", user.Username),
            new Claim("display_name", user.DisplayName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? "owner"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "MiniStore",
            audience: _configuration["Jwt:Audience"] ?? "MiniStoreClient",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
