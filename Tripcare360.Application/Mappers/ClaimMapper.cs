using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Features.Claim.Commands;
using Tripcare360.Domain.Entities.Claim;

namespace Tripcare360.Application.Mappers;

public static class ClaimMapper
{
    public static SubmitClaimCommand ToCommand(this SubmitClaimRequest r) => new(r);

    public static SubmitClaimResponse ToResponse(this ClaimEntity claim) =>
        new(claim.Id, claim.Status);
}
