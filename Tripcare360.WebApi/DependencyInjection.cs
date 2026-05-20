using Microsoft.Extensions.DependencyInjection;
using Tripcare360.Infrastructure.Services;

namespace Tripcare360.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddCors(o => o.AddPolicy("AllowNextJs", p =>
            p.WithOrigins("http://localhost:3002")
             .AllowAnyMethod()
             .AllowAnyHeader()));

        services.AddSingleton<ISseEventBroadcaster, SseEventBroadcaster>();

        return services;
    }
}
