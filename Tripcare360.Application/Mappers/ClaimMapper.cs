using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Domain.Entities.Claim;

namespace Tripcare360.Application.Mappers;

public static class ClaimMapper
{
    public static ReservationResponse ToReservationResponse(
        this ClaimEntity claim, string message, bool isOutageBypass) =>
        new(
            claim.ClaimCode,
            claim.CalculatedPayout,
            message,
            claim.CreatedAt.AddMinutes(10),
            isOutageBypass
        );

    public static FinalizeClaimResponse ToFinalizeResponse(this ClaimEntity claim) =>
        new(claim.ClaimCode, claim.Status, "Claim submitted successfully.");
}
