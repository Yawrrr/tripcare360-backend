namespace Tripcare360.Application.Dtos.Admin;

public record ManualValidationResponse(
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
    bool? IsBaggageDelayed,
    double? BaggageDelayHours,
    bool IsRouteMatch,
    bool IsEligible,
    string Verdict);
