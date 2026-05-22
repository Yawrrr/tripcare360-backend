using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Mappers;
using Tripcare360.Domain.Entities.Errors;

namespace Tripcare360.Application.Features.Claim.Queries;

public class GetClaimStatusQuery(string claimCode) : IRequest<ClaimStatusResponse>
{
    public string ClaimCode { get; } = claimCode;

    public class Validator : AbstractValidator<GetClaimStatusQuery>
    {
        public Validator() { RuleFor(q => q.ClaimCode).NotEmpty(); }
    }

    public class Handler(IClaimRepository claimRepository)
        : IRequestHandler<GetClaimStatusQuery, ClaimStatusResponse>
    {
        public async Task<ClaimStatusResponse> Handle(GetClaimStatusQuery query, CancellationToken ct)
        {
            var claim = await claimRepository.GetByClaimCodeAsync(query.ClaimCode);
            if (claim is null) throw new ApiException(ErrorCode.ClaimNotFound);
            return claim.ToStatusResponse();
        }
    }
}
