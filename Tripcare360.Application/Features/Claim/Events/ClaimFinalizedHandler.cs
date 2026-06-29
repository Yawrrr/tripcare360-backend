using MediatR;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Events;

public class ClaimFinalizedHandler(
    IClaimRepository claimRepository,
    IEmailNotificationService emailService)
    : INotificationHandler<ClaimFinalizedNotification>
{
    private static readonly HashSet<ClaimType> ApiVerifiableTypes =
    [
        ClaimType.FlightDelay,
    ];

    public async Task Handle(ClaimFinalizedNotification notification, CancellationToken ct)
    {
        await Task.Delay(5000, ct);

        var claim = await claimRepository.GetByClaimCodeAsync(notification.ClaimCode);
        if (claim is null) return;

        // Outage bypass: payout couldn't be calculated → always manual review
        if (claim.IsPreValidationFailedDueToOutage)
        {
            await Transition(claim, ClaimStatus.ManualReview, ct);
            return;
        }

        // Payout calculated as zero → genuinely unclaimable
        if (claim.CalculatedPayout == 0)
        {
            await Transition(claim, ClaimStatus.Rejected, ct);
            return;
        }

        if (ApiVerifiableTypes.Contains(claim.Type))
        {
            await Transition(claim, ClaimStatus.StpApproved, ct);
            return;
        }

        await Transition(claim, ClaimStatus.ManualReview, ct);
    }

    private async Task Transition(ClaimEntity claim, ClaimStatus status, CancellationToken ct)
    {
        claim.Status = status;
        await claimRepository.UpdateAsync(claim);

        if (status is ClaimStatus.StpApproved or ClaimStatus.Rejected)
            await emailService.SendClaimOutcomeEmailAsync(claim, ct);
    }
}
