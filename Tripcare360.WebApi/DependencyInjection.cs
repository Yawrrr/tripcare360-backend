using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Infrastructure.Services;

namespace Tripcare360.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddCors(o => o.AddPolicy("AllowNextJs", p =>
            p.WithOrigins("http://localhost:3002")
             .AllowAnyMethod()
             .AllowAnyHeader()));

        var jwtSecret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var jwtIssuer = config["Jwt:Issuer"] ?? "TripCare360";
        var jwtAudience = config["Jwt:Audience"] ?? "TripCare360Admin";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                };
            });

        services.AddAuthorization();
        services.AddSingleton<ISseEventBroadcaster, SseEventBroadcaster>();

        return services;
    }
}
