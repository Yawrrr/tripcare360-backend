namespace Tripcare360.Application.Dtos.Claim;

public record ClaimStatusResponse(
    string ClaimCode,
    string Status,
    decimal CalculatedPayout,
    string InsuredName,
    string PolicyNumber,
    string IdentityNumber,
    string ClaimType,
    string Route,
    decimal SubmittedAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    string? AdminComments);
