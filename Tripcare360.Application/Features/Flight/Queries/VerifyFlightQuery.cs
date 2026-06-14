using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Flight;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Application.Mappers;
using Tripcare360.Domain.Entities.Errors;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Flight.Queries;

// On-blur flight-level verification used to auto-fill the claim form, surface
// flight-delay eligibility, and validate the flight route against the policy.
// Booking + identity are validated later, at reserve.
public class VerifyFlightQuery(string flightNumber, TravelRoute route) : IRequest<VerifyFlightResponse>
{
    public string FlightNumber { get; } = flightNumber;
    public TravelRoute Route { get; } = route;

    public class Validator : AbstractValidator<VerifyFlightQuery>
    {
        public Validator() { RuleFor(q => q.FlightNumber).NotEmpty(); }
    }

    public class Handler(ITravelStatusRegistryClient flightRegistry)
        : IRequestHandler<VerifyFlightQuery, VerifyFlightResponse>
    {
        public async Task<VerifyFlightResponse> Handle(VerifyFlightQuery query, CancellationToken ct)
        {
            FlightStatusResponse? status;
            try
            {
                status = await flightRegistry.GetFlightStatusAsync(query.FlightNumber);
            }
            catch (Exception)
            {
                throw new ApiException(ErrorCode.ExternalServiceOutage);
            }

            if (status is null)
                throw new ApiException(ErrorCode.FlightNotFound);

            bool isEligible = status.IsDelayed && status.ActualDelayHours >= ClaimThresholds.FlightDelayMinHours;
            bool isRouteMatch = ClaimThresholds.IsFlightRouteMatch(status.DepartureCountry, status.ArrivalCountry, query.Route);
            return status.ToVerifyResponse(isEligible, isRouteMatch);
        }
    }
}
