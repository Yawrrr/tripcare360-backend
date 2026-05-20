using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Application.Mappers;
using System.Text.Json;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Entities.Errors;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Commands;

public class PreValidateAndReserveCommand(ReservationRequest request) : IRequest<ReservationResponse>
{
    public ReservationRequest Request { get; } = request;

    public class Validator : AbstractValidator<PreValidateAndReserveCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Request.PolicyNumber).NotEmpty();
            RuleFor(c => c.Request.IdentityNumber).NotEmpty();
            RuleFor(c => c.Request.InsuredName).NotEmpty();
            RuleFor(c => c.Request.IncidentDetailsJson).NotEmpty();
            RuleFor(c => c.Request.SubmittedAmount).GreaterThan(0);
        }
    }

    public class Handler(
        IClaimRepository claimRepository,
        ITravelStatusRegistryClient flightRegistry,
        IMinioStorageService minioStorage)
        : IRequestHandler<PreValidateAndReserveCommand, ReservationResponse>
    {
        private static readonly ClaimType[] FlightClaimTypes =
            [ClaimType.FlightDelay, ClaimType.BaggageDelay];

        public async Task<ReservationResponse> Handle(
            PreValidateAndReserveCommand command, CancellationToken cancellationToken)
        {
            var req = command.Request;
            bool isOutageBypass = false;

            if (FlightClaimTypes.Contains(req.ClaimType))
            {
                var incident = JsonSerializer.Deserialize<JsonElement>(req.IncidentDetailsJson);
                string flightNumber = incident.GetProperty("flightNumber").GetString() ?? string.Empty;
                DateTime departureDate = incident.GetProperty("departureDate").GetDateTime();

                try
                {
                    var flightStatus = await flightRegistry.GetFlightStatusAsync(flightNumber, departureDate);

                    if (flightStatus is null)
                        throw new ApiException(ErrorCode.AutomatedCheckFailed,
                            details: $"Flight {flightNumber} record not found.");

                    bool meetsThreshold = req.ClaimType == ClaimType.FlightDelay
                        ? flightStatus.ActualDelayHours >= 2
                        : flightStatus.BaggageDelayHours >= 6;

                    if (!meetsThreshold)
                        throw new ApiException(ErrorCode.AutomatedCheckFailed,
                            details: $"Flight {flightNumber} operated on schedule or delay does not meet the minimum threshold.");
                }
                catch (ApiException)
                {
                    throw;
                }
                catch (Exception)
                {
                    isOutageBypass = true;
                }
            }

            var claimCode = GenerateClaimCode();
            var fileKeys = new List<string>();

            foreach (var file in req.SupportingFiles)
                fileKeys.Add(await minioStorage.UploadFileAsync(claimCode, file));

            var claim = new ClaimEntity
            {
                ClaimCode = claimCode,
                PolicyNumber = req.PolicyNumber,
                IdentityNumber = req.IdentityNumber,
                InsuredName = req.InsuredName,
                Route = req.Route,
                Tier = req.Tier,
                Type = req.ClaimType,
                SubmittedAmount = req.SubmittedAmount,
                CalculatedPayout = CalculatePayout(req.ClaimType, req.SubmittedAmount, req.Tier),
                IncidentDetailsJson = req.IncidentDetailsJson,
                FileObjectKeys = fileKeys,
                IsPreValidationFailedDueToOutage = isOutageBypass,
                Status = ClaimStatus.Pending
            };

            await claimRepository.AddAsync(claim);

            string message = isOutageBypass
                ? "External validation service unavailable. Your claim has been saved for manual review."
                : "Pre-validation successful. Please review and submit within 10 minutes.";

            return claim.ToReservationResponse(message, isOutageBypass);
        }

        private static string GenerateClaimCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var suffix = new string(Enumerable.Range(0, 4)
                .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
            return $"CLM-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
        }

        private static decimal CalculatePayout(ClaimType claimType, decimal submittedAmount, PolicyTier tier)
        {
            // Placeholder — payout rules to be refined per Etiqa benefit matrix
            return submittedAmount;
        }
    }
}
