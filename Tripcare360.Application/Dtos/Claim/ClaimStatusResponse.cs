namespace Tripcare360.Application.Dtos.Claim;

public record ClaimStatusResponse(
    string ClaimCode,
    string Status,
    decimal CalculatedPayout,
    string Currency);
