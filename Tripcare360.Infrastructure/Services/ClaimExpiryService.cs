using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Infrastructure.Services;

public class ClaimExpiryService(
    IServiceScopeFactory scopeFactory,
    ILogger<ClaimExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ReservationWindow = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);
            await ExpireClaimsAsync(stoppingToken);
        }
    }

    private async Task ExpireClaimsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IClaimRepository>();

            var cutoff = DateTimeOffset.UtcNow - ReservationWindow;
            var expired = await repo.GetExpiredPendingAsync(cutoff, ct);

            foreach (var claim in expired)
            {
                claim.Status = ClaimStatus.Expired;
                await repo.UpdateAsync(claim);
                logger.LogInformation("[ClaimExpiry] Expired claim {ClaimCode}", claim.ClaimCode);
            }

            if (expired.Count > 0)
                logger.LogInformation("[ClaimExpiry] Expired {Count} claim(s).", expired.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "[ClaimExpiry] Error during expiry run.");
        }
    }
}
