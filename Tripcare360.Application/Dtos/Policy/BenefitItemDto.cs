namespace Tripcare360.Application.Dtos.Policy;

public record BenefitItemDto(
    string Category,
    string ClaimType,
    string DisplayName,
    decimal MaxPayout,
    string Notes,
    string Currency
);
