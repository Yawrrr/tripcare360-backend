namespace Tripcare360.Application.Dtos.Flight;

public record FlightStatusResponse(
    string FlightNumber,
    string AirlineName,
    string? DepartureDate,
    string? DepartureAirport,
    string? DepartureCountry,
    string? ArrivalDate,
    string? ArrivalAirport,
    string? ArrivalCountry,
    bool IsDelayed,
    double ActualDelayHours
);
