namespace Tripcare360.Application.Dtos.Admin;

public record ClaimDetailResponse(
    string ClaimCode,
    string PolicyNumber,
    string IdentityNumber,
    string InsuredName,
    string Route,
    string Tier,
    int InsuredAge,
    string Type,
    decimal SubmittedAmount,
    decimal CalculatedPayout,
    string IncidentDetailsJson,
    string Status,
    bool IsPreValidationFailedDueToOutage,
    string? AdminComments,
    DateTimeOffset? ProcessedAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<FileDetailItem> Files
);
