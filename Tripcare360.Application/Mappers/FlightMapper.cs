using Tripcare360.Application.Dtos.Flight;

namespace Tripcare360.Application.Mappers;

public static class FlightMapper
{
    public static VerifyFlightResponse ToVerifyResponse(this FlightStatusResponse status, bool isEligible, bool isRouteMatch) =>
        new(
            status.FlightNumber,
            status.AirlineName,
            status.DepartureDate,
            status.DepartureAirport,
            status.DepartureCountry,
            status.ArrivalDate,
            status.ArrivalAirport,
            status.ArrivalCountry,
            status.IsDelayed,
            status.ActualDelayHours,
            isEligible,
            isRouteMatch
        );
}
