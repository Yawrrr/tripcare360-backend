using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Application.Features.Benefits.Queries;

public class GetBenefitsQuery(GetBenefitsRequest request) : IRequest<List<BenefitItemDto>>
{
    public GetBenefitsRequest Request { get; } = request;

    public class Validator : AbstractValidator<GetBenefitsQuery>
    {
        public Validator() { }
    }

    public class Handler(IBenefitLimitsService benefitLimits)
        : IRequestHandler<GetBenefitsQuery, List<BenefitItemDto>>
    {
        public Task<List<BenefitItemDto>> Handle(GetBenefitsQuery query, CancellationToken ct)
        {
            var req = query.Request;
            var items = benefitLimits.GetAllBenefits(req.Route, req.Tier, req.InsuredAge, req.Country);
            return Task.FromResult(items.ToList());
        }
    }
}
