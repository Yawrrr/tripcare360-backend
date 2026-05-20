namespace Tripcare360.Application.Dtos.Claim;

public record FinalizeClaimRequest(string ClaimCode, bool AgreesToTermsAndConditions);
