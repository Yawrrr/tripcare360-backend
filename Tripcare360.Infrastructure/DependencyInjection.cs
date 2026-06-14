using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Infrastructure.Clients;
using Tripcare360.Infrastructure.Persistence;
using Tripcare360.Infrastructure.Repositories;
using Tripcare360.Infrastructure.Services;
using IPasswordHasher = Tripcare360.Application.Interfaces.Services.IPasswordHasher;

namespace Tripcare360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<Tripcare360DbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        var externalBaseUrl = config["ExternalServices:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalServices:BaseUrl is not configured.");

        services.AddHttpClient<IEtiqaBackendClient, EtiqaBackendClient>(client =>
            client.BaseAddress = new Uri(externalBaseUrl));

        services.AddHttpClient<ITravelStatusRegistryClient, TravelStatusRegistryClient>(client =>
            client.BaseAddress = new Uri(externalBaseUrl));

        services.AddMinio(opts => opts
            .WithEndpoint(config["MinIO:Endpoint"] ?? "localhost:9000")
            .WithCredentials(
                config["MinIO:AccessKey"] ?? "tripcareAdmin",
                config["MinIO:SecretKey"] ?? "tripcarePassword123")
            .WithSSL(false)
            .Build());

        services.AddScoped<IMinioStorageService, MinioStorageService>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<DbSeeder>();
        services.AddHostedService<ClaimExpiryService>();

        return services;
    }
}
