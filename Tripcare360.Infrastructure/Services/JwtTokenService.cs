using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Admin;

namespace Tripcare360.Infrastructure.Services;

public class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    public string GenerateToken(AdminUserEntity user, out DateTimeOffset expiresAt)
    {
        var secret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer          = config["Jwt:Issuer"]          ?? "tripcare360-api";
        var audience        = config["Jwt:Audience"]        ?? "tripcare360-web";
        var lifetimeMinutes = int.Parse(config["Jwt:LifetimeMinutes"] ?? "480");

        expiresAt = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes);

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            expiresAt.UtcDateTime,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
