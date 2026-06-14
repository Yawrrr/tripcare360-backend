using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Infrastructure.Services;

namespace Tripcare360.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddEndpointsApiExplorer();

        services.AddCors(o => o.AddPolicy("AllowNextJs", p =>
            p.WithOrigins("http://localhost:3002")
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials()));

        var jwtSecret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey  = true,
                    ValidIssuer              = config["Jwt:Issuer"]   ?? "tripcare360-api",
                    ValidAudience            = config["Jwt:Audience"] ?? "tripcare360-web",
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew                = TimeSpan.FromSeconds(30),
                };
                opts.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode  = 401;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync(
                            """{"status":"Failed","errorCode":"AUTH_003","errMsg":"Unauthorized. Token is missing or expired.","details":"","timestamp":"","traceId":""}""");
                    },
                };
            });

        services.AddAuthorization();
        services.AddSingleton<ISseEventBroadcaster, SseEventBroadcaster>();

        return services;
    }
}
