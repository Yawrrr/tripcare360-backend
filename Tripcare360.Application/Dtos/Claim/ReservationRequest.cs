using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Claim;

public record ReservationRequest(
    string PolicyNumber,
    string IdentityNumber,
    string InsuredName,
    TravelRoute Route,
    PolicyTier Tier,
    int InsuredAge,
    ClaimType ClaimType,
    decimal SubmittedAmount,
    string IncidentDetailsJson,
    List<ClaimFileUpload> SupportingFiles,
    string Country
);
