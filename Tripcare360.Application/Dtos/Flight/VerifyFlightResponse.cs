namespace Tripcare360.Application.Dtos.Flight;

// Frontend-facing flight verification result used to auto-fill the claim details form.
public record VerifyFlightResponse(
    string FlightNumber,
    string AirlineName,
    string? DepartureDate,
    string? DepartureAirport,
    string? DepartureCountry,
    string? ArrivalDate,
    string? ArrivalAirport,
    string? ArrivalCountry,
    bool IsDelayed,
    double ActualDelayHours,
    bool IsEligible,
    bool IsRouteMatch
);
