using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Features.Flight;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Application.Mappers;
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
            RuleFor(c => c.Request.SubmittedAmount).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Request.Country).NotEmpty();
        }
    }

    public class Handler(
        IClaimRepository claimRepository,
        ITravelStatusRegistryClient flightRegistry,
        IMinioStorageService minioStorage,
        IBenefitLimitsService benefitLimits,
        ILogger<Handler> logger)
        : IRequestHandler<PreValidateAndReserveCommand, ReservationResponse>
    {
        private static readonly ClaimType[] FlightClaimTypes =
            [ClaimType.FlightDelay, ClaimType.BaggageDelay];

        public async Task<ReservationResponse> Handle(
            PreValidateAndReserveCommand command, CancellationToken cancellationToken)
        {
            var req = command.Request;
            bool isOutageBypass = false;
            double actualDelayHours = 0;

            if (FlightClaimTypes.Contains(req.ClaimType))
            {
                var incident = JsonSerializer.Deserialize<JsonElement>(req.IncidentDetailsJson);
                string flightNumber = incident.GetProperty("flightNumber").GetString() ?? string.Empty;
                string bookingNumber = incident.TryGetProperty("bookingNumber", out var bk)
                    ? bk.GetString() ?? string.Empty
                    : string.Empty;

                try
                {
                    // Booking lookup = proof the passenger boarded + the owning identity.
                    var booking = await flightRegistry.GetBookingAsync(flightNumber, bookingNumber);

                    if (booking is null)
                        throw new ApiException(ErrorCode.BookingNotFound);

                    if (!string.Equals(booking.IdentityNumber, req.IdentityNumber, StringComparison.OrdinalIgnoreCase))
                        throw new ApiException(ErrorCode.TravellerMismatch);

                    // Flight status needed for: route validation (both types) + delay threshold (FlightDelay only).
                    var flightStatus = await flightRegistry.GetFlightStatusAsync(flightNumber);

                    if (flightStatus is null)
                        throw new ApiException(ErrorCode.FlightNotFound);

                    if (!ClaimThresholds.IsFlightRouteMatch(flightStatus.DepartureCountry, flightStatus.ArrivalCountry, req.Route))
                        throw new ApiException(ErrorCode.FlightRouteMismatch);

                    if (req.ClaimType == ClaimType.FlightDelay)
                    {
                        if (!(flightStatus.IsDelayed && flightStatus.ActualDelayHours >= ClaimThresholds.FlightDelayMinHours))
                            throw new ApiException(ErrorCode.FlightDelayThresholdNotMet);

                        actualDelayHours = flightStatus.ActualDelayHours;
                    }
                    else // BaggageDelay
                    {
                        if (!(booking.IsBaggageDelayed && booking.BaggageDelayHours >= ClaimThresholds.BaggageDelayMinHours))
                            throw new ApiException(ErrorCode.BaggageDelayThresholdNotMet);

                        actualDelayHours = booking.BaggageDelayHours;
                    }
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
            var fileLabels = new List<string>();

            foreach (var file in req.SupportingFiles)
            {
                if (await IsPdfEncryptedAsync(file, cancellationToken))
                    throw new ApiException(ErrorCode.PdfEncrypted);
            }

            foreach (var file in req.SupportingFiles)
            {
                fileKeys.Add(await minioStorage.UploadFileAsync(claimCode, file));
                fileLabels.Add(file.Label);
            }

            var calculatedPayout = CalculatePayout(req, actualDelayHours, benefitLimits, req.Country);

            logger.LogInformation(
                "[PreValidate] claimType={ClaimType} route={Route} tier={Tier} insuredAge={InsuredAge} actualDelayHours={ActualDelayHours} isOutageBypass={IsOutageBypass} calculatedPayout={CalculatedPayout}",
                req.ClaimType, req.Route, req.Tier, req.InsuredAge, actualDelayHours, isOutageBypass, calculatedPayout);

            var claim = new ClaimEntity
            {
                ClaimCode = claimCode,
                PolicyNumber = req.PolicyNumber,
                IdentityNumber = req.IdentityNumber,
                InsuredName = req.InsuredName,
                InsuredEmail = req.InsuredEmail,
                Route = req.Route,
                Tier = req.Tier,
                InsuredAge = req.InsuredAge,
                Type = req.ClaimType,
                SubmittedAmount = req.SubmittedAmount,
                CalculatedPayout = calculatedPayout,
                IncidentDetailsJson = req.IncidentDetailsJson,
                FileObjectKeys = fileKeys,
                FileLabels = fileLabels,
                Country = req.Country,
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
            return $"CLM{suffix}";
        }

        private static decimal CalculatePayout(
            ReservationRequest req, double actualDelayHours, IBenefitLimitsService limits, string country)
        {
            if (req.ClaimType == ClaimType.FlightDelay)
            {
                var (ratePerBlock, blockSize, maxPayout) = limits.GetFlightDelayRate(req.Route, req.Tier, country);
                if (ratePerBlock == 0) return 0;
                var blocks = Math.Max(1, Math.Floor(actualDelayHours / (double)blockSize));
                return Math.Min((decimal)blocks * ratePerBlock, maxPayout);
            }

            if (req.ClaimType == ClaimType.HospitalConfinement)
            {
                var (dailyRate, _, maxPayout) = limits.GetConfinementRate(req.Route, req.Tier, country);
                int days = ParseIntField(req.IncidentDetailsJson, "numberOfDays");
                return Math.Min(days * dailyRate, maxPayout);
            }

            if (req.ClaimType == ClaimType.HijackInconvenience)
            {
                var (dailyRate, _, maxPayout) = limits.GetHijackRate(req.Route, req.Tier, country);
                int days = ParseIntField(req.IncidentDetailsJson, "durationDays");
                return Math.Min(days * dailyRate, maxPayout);
            }

            if (req.ClaimType is ClaimType.BaggageDelay or ClaimType.MissedConnection)
                return limits.GetMaxPayout(req.Route, req.Tier, req.ClaimType, req.InsuredAge, country);

            var cap = limits.GetMaxPayout(req.Route, req.Tier, req.ClaimType, req.InsuredAge, country);
            return cap > 0 ? Math.Min(req.SubmittedAmount, cap) : req.SubmittedAmount;
        }

        private static int ParseIntField(string json, string field)
        {
            try
            {
                var doc = JsonSerializer.Deserialize<JsonElement>(json);
                return doc.GetProperty(field).GetInt32();
            }
            catch
            {
                return 0;
            }
        }

        private static async Task<bool> IsPdfEncryptedAsync(ClaimFileUpload file, CancellationToken ct)
        {
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return false;
            // /Encrypt lives in the PDF trailer dictionary near the END of the file.
            // Reading only the header misses it, so we read the full content.
            using var reader = new StreamReader(file.Content, Encoding.Latin1, leaveOpen: true);
            var content = await reader.ReadToEndAsync(ct);
            if (file.Content.CanSeek) file.Content.Seek(0, SeekOrigin.Begin);
            return content.Contains("/Encrypt");
        }
    }
}
