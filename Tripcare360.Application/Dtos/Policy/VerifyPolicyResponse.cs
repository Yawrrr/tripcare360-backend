using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Policy;

public record VerifyPolicyResponse(
    string PolicyNumber,
    string InsuredName,
    string IdentityNumber,
    int InsuredAge,
    TravelRoute Route,
    PolicyTier Tier,
    Country Country,
    DateTime StartDate,
    DateTime EndDate,
    List<string> EligibleClaimTypes,
    bool HasCovid19Coverage
);
