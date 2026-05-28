using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string GenerateToken(string username)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer = config["Jwt:Issuer"] ?? "TripCare360";
        var audience = config["Jwt:Audience"] ?? "TripCare360Admin";
        var expiryHours = int.TryParse(config["Jwt:ExpiryHours"], out var h) ? h : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: [new Claim(ClaimTypes.Name, username)],
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
