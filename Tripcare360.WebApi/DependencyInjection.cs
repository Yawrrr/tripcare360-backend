using Microsoft.Extensions.DependencyInjection;

namespace Tripcare360.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddCors(o => o.AddPolicy("AllowNextJs", p =>
            p.WithOrigins("http://localhost:3000")
             .AllowAnyMethod()
             .AllowAnyHeader()));
        return services;
    }
}
