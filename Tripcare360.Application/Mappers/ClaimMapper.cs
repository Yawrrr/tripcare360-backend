using Tripcare360.Application.Dtos.Admin;
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

    public static ClaimStatusResponse ToStatusResponse(this ClaimEntity claim) =>
        new(claim.ClaimCode, claim.Status.ToString(), claim.CalculatedPayout);

    public static ClaimListItemResponse ToListItemResponse(this ClaimEntity claim) =>
        new(
            claim.ClaimCode,
            claim.PolicyNumber,
            claim.InsuredName,
            claim.Type.ToString(),
            claim.Status.ToString(),
            claim.CalculatedPayout,
            claim.CreatedAt,
            claim.IsPreValidationFailedDueToOutage
        );

    public static ClaimDetailResponse ToDetailResponse(
        this ClaimEntity claim, IReadOnlyList<FileDetailItem> files) =>
        new(
            claim.ClaimCode,
            claim.PolicyNumber,
            claim.IdentityNumber,
            claim.InsuredName,
            claim.Route.ToString(),
            claim.Tier.ToString(),
            claim.InsuredAge,
            claim.Type.ToString(),
            claim.SubmittedAmount,
            claim.CalculatedPayout,
            claim.IncidentDetailsJson,
            claim.Status.ToString(),
            claim.IsPreValidationFailedDueToOutage,
            claim.AdminComments,
            claim.ProcessedAt,
            claim.CreatedAt,
            files
        );

    public static UpdateClaimStatusResponse ToUpdateStatusResponse(this ClaimEntity claim) =>
        new(claim.ClaimCode, claim.Status.ToString());
}
