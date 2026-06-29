using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Dtos.Flight;
using Tripcare360.Application.Features.Claim.Commands;
using Tripcare360.Application.Features.Claim.Queries;
using Tripcare360.Application.Features.Flight.Queries;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.WebApi.Models;
using System.Collections.Generic;

namespace Tripcare360.WebApi.Controllers;

[ApiController]
[Route("api/claim")]
public class ClaimsController(ISender sender, ISseEventBroadcaster sseBroadcaster) : ControllerBase
{
    [HttpPost("verify")]
    public async Task<ReservationResponse> Reserve(
        [FromForm] ReservationFormInput input, CancellationToken ct)
    {
        var labels = input.SupportingFileLabels ?? [];
        var files = input.SupportingFiles?
            .Select((f, i) => new ClaimFileUpload(
                f.FileName, f.ContentType, f.OpenReadStream(), f.Length,
                i < labels.Count ? labels[i] : "Supporting Document"))
            .ToList() ?? [];

        var request = new ReservationRequest(
            input.PolicyNumber,
            input.IdentityNumber,
            input.InsuredName,
            input.Route,
            input.Tier,
            input.InsuredAge,
            input.ClaimType,
            input.SubmittedAmount,
            input.IncidentDetailsJson,
            files,
            input.Country,
            input.InsuredEmail);

        return await sender.Send(new PreValidateAndReserveCommand(request), ct);
    }

    [HttpPost("verify-flight")]
    public async Task<VerifyFlightResponse> VerifyFlight(
        [FromBody] VerifyFlightRequest request, CancellationToken ct)
        => await sender.Send(new VerifyFlightQuery(request.FlightNumber, request.Route), ct);

    [HttpPost]
    public async Task<FinalizeClaimResponse> Finalize(
        [FromBody] FinalizeClaimRequest request, CancellationToken ct)
        => await sender.Send(new FinalizeClaimCommand(request), ct);

    [HttpGet("history")]
    public async Task<IReadOnlyList<ClaimHistoryItemResponse>> GetClaimHistory(
        [FromQuery] string identityNumber, CancellationToken ct)
        => await sender.Send(new GetClaimHistoryQuery(identityNumber), ct);

    [HttpGet("{claimCode}")]
    public async Task<ClaimStatusResponse> GetStatus(string claimCode, CancellationToken ct)
        => await sender.Send(new GetClaimStatusQuery(claimCode), ct);

    [HttpGet("sse/{claimCode}")]
    public async Task Stream(string claimCode, CancellationToken ct)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        sseBroadcaster.RegisterClient(claimCode, async (eventType, data) =>
        {
            await Response.WriteAsync($"event: {eventType}\n", ct);
            await Response.WriteAsync($"data: {data}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        });

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (TaskCanceledException) { }
        finally
        {
            sseBroadcaster.UnregisterClient(claimCode);
        }
    }
}
