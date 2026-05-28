using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tripcare360.Application.Dtos.Auth;
using Tripcare360.Application.Features.Auth.Commands;

namespace Tripcare360.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        => Ok(await sender.Send(new LoginCommand(req), ct));
}
