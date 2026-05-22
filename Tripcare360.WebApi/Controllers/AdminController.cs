using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tripcare360.Application.Dtos.Admin;
using Tripcare360.Application.Features.Claim.Commands;
using Tripcare360.Application.Features.Claim.Queries;
using Tripcare360.Domain.Enums;

namespace Tripcare360.WebApi.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(ISender sender) : ControllerBase
{
    [HttpGet("claims")]
    public async Task<IActionResult> ListClaims(
        [FromQuery] ClaimStatus? status, CancellationToken ct)
        => Ok(await sender.Send(new ListClaimsQuery(status), ct));

    [HttpGet("claims/{claimCode}")]
    public async Task<IActionResult> GetClaimDetail(
        string claimCode, CancellationToken ct)
        => Ok(await sender.Send(new GetAdminClaimDetailQuery(claimCode), ct));

    [HttpPost("claims/{claimCode}/action")]
    public async Task<IActionResult> UpdateClaimStatus(
        string claimCode, [FromBody] UpdateClaimStatusRequest req, CancellationToken ct)
        => Ok(await sender.Send(new UpdateClaimStatusCommand(claimCode, req), ct));
}
