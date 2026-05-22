using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Admin;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Mappers;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Queries;

public class ListClaimsQuery(ClaimStatus? status) : IRequest<IReadOnlyList<ClaimListItemResponse>>
{
    public ClaimStatus? Status { get; } = status;

    public class Validator : AbstractValidator<ListClaimsQuery>
    {
        public Validator() { }
    }

    public class Handler(IClaimRepository repo)
        : IRequestHandler<ListClaimsQuery, IReadOnlyList<ClaimListItemResponse>>
    {
        public async Task<IReadOnlyList<ClaimListItemResponse>> Handle(
            ListClaimsQuery query, CancellationToken ct)
        {
            var all = await repo.GetAllAsync(ct);
            var filtered = query.Status.HasValue
                ? all.Where(c => c.Status == query.Status.Value)
                : all;
            return filtered
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => c.ToListItemResponse())
                .ToList()
                .AsReadOnly();
        }
    }
}
