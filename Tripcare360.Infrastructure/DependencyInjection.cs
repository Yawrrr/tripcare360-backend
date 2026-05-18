using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Infrastructure.Persistence;
using Tripcare360.Infrastructure.Repositories;
using Tripcare360.Infrastructure.Services;

namespace Tripcare360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<Tripcare360DbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRojakkkService, RojakkkService>();
        services.AddScoped<IClaimRepository, ClaimRepository>();

        return services;
    }
}
