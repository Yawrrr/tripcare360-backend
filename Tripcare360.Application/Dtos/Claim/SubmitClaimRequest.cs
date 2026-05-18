using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Claim;

public record SubmitClaimRequest(
    string PolicyNumber,
    string IdentityNumber,
    string FlightNumber,
    ClaimType ClaimType,
    DateTime FlightDate
);
