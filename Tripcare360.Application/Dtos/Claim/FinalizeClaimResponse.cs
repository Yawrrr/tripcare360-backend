using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Claim;

public record FinalizeClaimResponse(string ClaimCode, ClaimStatus Status, string Message);
