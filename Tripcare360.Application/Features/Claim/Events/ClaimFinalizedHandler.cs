using MediatR;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Events;

public class ClaimFinalizedHandler(IClaimRepository claimRepository)
    : INotificationHandler<ClaimFinalizedNotification>
{
    private static readonly HashSet<ClaimType> ApiVerifiableTypes =
    [
        ClaimType.FlightDelay,
        ClaimType.BaggageDelay,
        ClaimType.MissedConnection,
    ];

    public async Task Handle(ClaimFinalizedNotification notification, CancellationToken ct)
    {
        var claim = await claimRepository.GetByClaimCodeAsync(notification.ClaimCode);
        if (claim is null) return;

        // Outage bypass: payout couldn't be calculated → always manual review
        if (claim.IsPreValidationFailedDueToOutage)
        {
            await Transition(claim, ClaimStatus.ManualReview);
            return;
        }

        // Payout calculated as zero → genuinely unclaimable
        if (claim.CalculatedPayout == 0)
        {
            await Transition(claim, ClaimStatus.Rejected);
            return;
        }

        if (ApiVerifiableTypes.Contains(claim.Type))
        {
            await Transition(claim, ClaimStatus.StpApproved);
            return;
        }

        await Transition(claim, ClaimStatus.ManualReview);
    }

    private async Task Transition(ClaimEntity claim, ClaimStatus status)
    {
        claim.Status = status;
        await claimRepository.UpdateAsync(claim);
    }
}
