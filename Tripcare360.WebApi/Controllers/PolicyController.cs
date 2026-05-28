using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Features.Benefits.Queries;
using Tripcare360.Application.Features.Policy.Commands;
using Tripcare360.Domain.Enums;

namespace Tripcare360.WebApi.Controllers;

[ApiController]
[Route("api/policy")]
public class PolicyController(ISender sender) : ControllerBase
{
    [HttpPost("verify")]
    public async Task<VerifyPolicyResponse> Verify(
        [FromBody] VerifyPolicyRequest request, CancellationToken ct)
        => await sender.Send(new VerifyPolicyCommand(request), ct);

    [HttpGet("benefits")]
    public async Task<List<BenefitItemDto>> GetBenefits(
        [FromQuery] TravelRoute route,
        [FromQuery] PolicyTier tier,
        [FromQuery] Country country,
        [FromQuery] int insuredAge = 0,
        CancellationToken ct = default)
        => await sender.Send(new GetBenefitsQuery(new GetBenefitsRequest(route, tier, country, insuredAge)), ct);
}
