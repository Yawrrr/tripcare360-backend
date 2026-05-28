using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Application.Mappers;
using System.Text.Json;
using Tripcare360.Domain.Common;
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
        }
    }

    public class Handler(
        IClaimRepository claimRepository,
        ITravelStatusRegistryClient flightRegistry,
        IMinioStorageService minioStorage,
        IBenefitLimitsService benefitLimits,
        IEtiqaBackendClient etiqaClient,
        IDocumentTextExtractor textExtractor,
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

                    actualDelayHours = req.ClaimType == ClaimType.FlightDelay
                        ? flightStatus.ActualDelayHours
                        : flightStatus.BaggageDelayHours;
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

            // ── Read file bytes first so we can both extract text and upload ────
            var fileData = new List<(ClaimFileUpload File, byte[] Bytes)>();
            foreach (var file in req.SupportingFiles)
            {
                var bytes = new byte[file.Length];
                _ = await file.Content.ReadAsync(bytes, cancellationToken);
                fileData.Add((file, bytes));
            }

            // ── Build AI document payloads ────────────────────────────────────
            var docPayloads = new List<DocumentPayload>();
            foreach (var (file, bytes) in fileData)
            {
                var isPdf = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                            || file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

                string text = string.Empty;
                if (isPdf)
                {
                    using var pdfStream = new MemoryStream(bytes);
                    text = textExtractor.ExtractText(pdfStream);
                }

                byte[]? imageBytes = string.IsNullOrWhiteSpace(text) ? bytes : null;
                docPayloads.Add(new DocumentPayload(text, imageBytes, file.FileName, file.ContentType));
            }

            // ── Upload files to MinIO ─────────────────────────────────────────
            var claimCode = GenerateClaimCode();
            var fileKeys = new List<string>();
            var fileLabels = new List<string>();

            foreach (var (file, bytes) in fileData)
            {
                using var uploadStream = new MemoryStream(bytes);
                var uploadFile = file with { Content = uploadStream, Length = bytes.Length };
                fileKeys.Add(await minioStorage.UploadFileAsync(claimCode, uploadFile));
                fileLabels.Add(file.Label);
            }

            // ── AI document processing (non-blocking — failure never rejects claim) ──
            string? aiResult = null;
            if (docPayloads.Count > 0)
                aiResult = await etiqaClient.ProcessClaimDocumentsAsync(req.ClaimType.ToString(), docPayloads);

            var calculatedPayout = CalculatePayout(req, actualDelayHours, benefitLimits);

            logger.LogInformation(
                "[PreValidate] claimType={ClaimType} route={Route} tier={Tier} insuredAge={InsuredAge} actualDelayHours={ActualDelayHours} isOutageBypass={IsOutageBypass} calculatedPayout={CalculatedPayout} aiProcessed={AiProcessed}",
                req.ClaimType, req.Route, req.Tier, req.InsuredAge, actualDelayHours, isOutageBypass, calculatedPayout, aiResult is not null);

            var claim = new ClaimEntity
            {
                ClaimCode = claimCode,
                PolicyNumber = req.PolicyNumber,
                IdentityNumber = req.IdentityNumber,
                InsuredName = req.InsuredName,
                Route = req.Route,
                Tier = req.Tier,
                InsuredAge = req.InsuredAge,
                Country = req.Country,
                Currency = CountryCurrencyMap.GetCurrency(req.Country),
                Type = req.ClaimType,
                SubmittedAmount = req.SubmittedAmount,
                CalculatedPayout = calculatedPayout,
                IncidentDetailsJson = req.IncidentDetailsJson,
                FileObjectKeys = fileKeys,
                FileLabels = fileLabels,
                IsPreValidationFailedDueToOutage = isOutageBypass,
                AiExtractionResultJson = aiResult,
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
            ReservationRequest req, double actualDelayHours, IBenefitLimitsService limits)
        {
            if (req.ClaimType == ClaimType.FlightDelay)
            {
                var (ratePerBlock, blockSize, maxPayout) = limits.GetFlightDelayRate(req.Country, req.Route, req.Tier);
                if (ratePerBlock == 0) return 0;
                var blocks = Math.Max(1, Math.Floor(actualDelayHours / (double)blockSize));
                return Math.Min((decimal)blocks * ratePerBlock, maxPayout);
            }

            if (req.ClaimType == ClaimType.HospitalConfinement)
            {
                var (dailyRate, _, maxPayout) = limits.GetConfinementRate(req.Country, req.Route, req.Tier);
                int days = ParseIntField(req.IncidentDetailsJson, "numberOfDays");
                return Math.Min(days * dailyRate, maxPayout);
            }

            if (req.ClaimType == ClaimType.HijackInconvenience)
            {
                var (dailyRate, _, maxPayout) = limits.GetHijackRate(req.Country, req.Route, req.Tier);
                int days = ParseIntField(req.IncidentDetailsJson, "durationDays");
                return Math.Min(days * dailyRate, maxPayout);
            }

            if (req.ClaimType is ClaimType.BaggageDelay or ClaimType.MissedConnection)
                return limits.GetMaxPayout(req.Country, req.Route, req.Tier, req.ClaimType, req.InsuredAge);

            var cap = limits.GetMaxPayout(req.Country, req.Route, req.Tier, req.ClaimType, req.InsuredAge);
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
    }
}
