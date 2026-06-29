namespace Tripcare360.Application.Dtos.Admin;

public record ClaimListItemResponse(
    string ClaimCode,
    string PolicyNumber,
    string InsuredName,
    string Type,
    string Status,
    decimal CalculatedPayout,
    DateTimeOffset CreatedAt,
    bool IsPreValidationFailedDueToOutage,
    string CountryCode
);
