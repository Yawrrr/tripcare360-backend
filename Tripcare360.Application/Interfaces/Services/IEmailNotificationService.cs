using Tripcare360.Domain.Entities.Claim;

namespace Tripcare360.Application.Interfaces.Services;

public interface IEmailNotificationService
{
    Task SendClaimOutcomeEmailAsync(ClaimEntity claim, CancellationToken ct = default);
}
