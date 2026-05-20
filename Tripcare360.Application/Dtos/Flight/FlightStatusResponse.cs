namespace Tripcare360.Application.Dtos.Flight;

public record FlightStatusResponse(
    string FlightNumber,
    DateTime DepartureDate,
    string AirlineName,
    bool IsDelayed,
    int ActualDelayHours,
    bool IsBaggageDelayed,
    int BaggageDelayHours
);
