namespace Tripcare360.Application.Dtos.Claim;

public record ClaimHistoryItemResponse(
    string ClaimCode,
    string Type,
    string Status,
    decimal CalculatedPayout,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt
);
