using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Features.Policy.Commands;

namespace Tripcare360.WebApi.Controllers;

[ApiController]
[Route("api/policy")]
public class PolicyController(ISender sender) : ControllerBase
{
    [HttpPost("verify")]
    public async Task<VerifyPolicyResponse> Verify(
        [FromBody] VerifyPolicyRequest request, CancellationToken ct)
        => await sender.Send(new VerifyPolicyCommand(request), ct);
}
