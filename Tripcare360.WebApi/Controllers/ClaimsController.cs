using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Mappers;

namespace Tripcare360.WebApi.Controllers;

[ApiController]
[Route("api/claims")]
public class ClaimsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<SubmitClaimResponse> Submit(
        [FromBody] SubmitClaimRequest request, CancellationToken ct)
        => await sender.Send(request.ToCommand(), ct);
}
