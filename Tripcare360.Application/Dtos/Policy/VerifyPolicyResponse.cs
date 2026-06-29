using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Policy;

public record VerifyPolicyResponse(
    string PolicyNumber,
    string InsuredName,
    string IdentityNumber,
    int InsuredAge,
    TravelRoute Route,
    PolicyTier Tier,
    DateTime StartDate,
    DateTime EndDate,
    List<string> EligibleClaimTypes,
    string? InsuredEmail
);
