using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Mappers;

namespace Tripcare360.Application.Features.Claim.Queries;

public class GetClaimHistoryQuery(string identityNumber) : IRequest<IReadOnlyList<ClaimHistoryItemResponse>>
{
    public string IdentityNumber { get; } = identityNumber;

    public class Validator : AbstractValidator<GetClaimHistoryQuery>
    {
        public Validator() { RuleFor(q => q.IdentityNumber).NotEmpty(); }
    }

    public class Handler(IClaimRepository claimRepository)
        : IRequestHandler<GetClaimHistoryQuery, IReadOnlyList<ClaimHistoryItemResponse>>
    {
        public async Task<IReadOnlyList<ClaimHistoryItemResponse>> Handle(
            GetClaimHistoryQuery query, CancellationToken ct)
        {
            var claims = await claimRepository.GetByIdentityNumberAsync(query.IdentityNumber, ct);
            return claims.Select(c => c.ToHistoryItemResponse()).ToList();
        }
    }
}
