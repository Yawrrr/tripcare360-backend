namespace Tripcare360.Application.Dtos.Claim;

public record ReservationResponse(
    string ClaimCode,
    decimal CalculatedPayout,
    string ValidationMessage,
    DateTimeOffset ExpiresAt,
    bool IsOutageBypassTransition,
    string Currency
);
