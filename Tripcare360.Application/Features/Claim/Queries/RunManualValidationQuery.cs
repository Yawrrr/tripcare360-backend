using System.Text.Json;
using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Admin;
using Tripcare360.Application.Features.Flight;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Errors;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Queries;

public class RunManualValidationQuery(string claimCode) : IRequest<ManualValidationResponse>
{
    public string ClaimCode { get; } = claimCode;

    public class Validator : AbstractValidator<RunManualValidationQuery>
    {
        public Validator() { RuleFor(q => q.ClaimCode).NotEmpty(); }
    }

    public class Handler(IClaimRepository repo, ITravelStatusRegistryClient flightRegistry)
        : IRequestHandler<RunManualValidationQuery, ManualValidationResponse>
    {
        public async Task<ManualValidationResponse> Handle(
            RunManualValidationQuery query, CancellationToken ct)
        {
            var claim = await repo.GetByClaimCodeAsync(query.ClaimCode);
            if (claim is null) throw new ApiException(ErrorCode.ClaimNotFound);

            var incident = JsonSerializer.Deserialize<JsonElement>(claim.IncidentDetailsJson);
            var flightNumber = incident.GetProperty("flightNumber").GetString() ?? string.Empty;
            var bookingNumber = incident.TryGetProperty("bookingNumber", out var bk)
                ? bk.GetString() ?? string.Empty
                : string.Empty;

            var flight = await flightRegistry.GetFlightStatusAsync(flightNumber);
            if (flight is null)
                throw new ApiException(ErrorCode.AutomatedCheckFailed,
                    details: $"Flight {flightNumber} not found in the registry.");

            var booking = await flightRegistry.GetBookingAsync(flightNumber, bookingNumber);
            if (booking is null)
                throw new ApiException(ErrorCode.AutomatedCheckFailed,
                    details: $"No boarding record found for flight {flightNumber} and booking {bookingNumber}.");

            bool isRouteMatch = ClaimThresholds.IsFlightRouteMatch(
                flight.DepartureCountry, flight.ArrivalCountry, claim.Route);

            bool isBaggageClaim = claim.Type == ClaimType.BaggageDelay;

            bool isEligible;
            string verdict;

            if (!isRouteMatch)
            {
                bool flightIsDomestic = string.Equals(
                    flight.DepartureCountry, flight.ArrivalCountry,
                    StringComparison.OrdinalIgnoreCase);
                var flightRouteLabel = flightIsDomestic ? "domestic" : "international";
                var policyRouteLabel = claim.Route == TravelRoute.Domestic ? "domestic" : "international";
                isEligible = false;
                verdict = $"Route mismatch — this is a {flightRouteLabel} flight but the policy covers {policyRouteLabel} travel.";
            }
            else if (isBaggageClaim)
            {
                if (!booking.IsBaggageDelayed)
                {
                    isEligible = false;
                    verdict = "Baggage delay not confirmed by registry. Not eligible.";
                }
                else if (booking.BaggageDelayHours < ClaimThresholds.BaggageDelayMinHours)
                {
                    isEligible = false;
                    verdict = $"Baggage delayed {booking.BaggageDelayHours:F1}h — below the {ClaimThresholds.BaggageDelayMinHours}h minimum. Not eligible.";
                }
                else
                {
                    isEligible = true;
                    verdict = $"Baggage delayed {booking.BaggageDelayHours:F1}h — exceeds the minimum threshold. Eligible for payout.";
                }
            }
            else // FlightDelay
            {
                if (!flight.IsDelayed || flight.ActualDelayHours < ClaimThresholds.FlightDelayMinHours)
                {
                    isEligible = false;
                    verdict = $"Flight delayed {flight.ActualDelayHours:F1}h — below the {ClaimThresholds.FlightDelayMinHours}h minimum. Not eligible.";
                }
                else
                {
                    isEligible = true;
                    verdict = $"Flight delayed {flight.ActualDelayHours:F1}h — exceeds the minimum threshold. Eligible for payout.";
                }
            }

            return new ManualValidationResponse(
                flight.FlightNumber,
                flight.AirlineName,
                flight.DepartureDate,
                flight.DepartureAirport,
                flight.DepartureCountry,
                flight.ArrivalDate,
                flight.ArrivalAirport,
                flight.ArrivalCountry,
                flight.IsDelayed,
                flight.ActualDelayHours,
                isBaggageClaim ? booking.IsBaggageDelayed : null,
                isBaggageClaim ? booking.BaggageDelayHours : null,
                isRouteMatch,
                isEligible,
                verdict);
        }
    }
}
