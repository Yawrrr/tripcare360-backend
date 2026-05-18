using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Claim;

public record SubmitClaimResponse(Guid ClaimId, ClaimStatus Status);
