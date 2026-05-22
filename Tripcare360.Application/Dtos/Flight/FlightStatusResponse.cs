namespace Tripcare360.Application.Dtos.Flight;

public record FlightStatusResponse(
    string FlightNumber,
    DateTime DepartureDate,
    string AirlineName,
    bool IsDelayed,
    double ActualDelayHours,
    bool IsBaggageDelayed,
    double BaggageDelayHours
);
