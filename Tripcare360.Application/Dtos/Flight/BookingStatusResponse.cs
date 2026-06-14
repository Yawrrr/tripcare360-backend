namespace Tripcare360.Application.Dtos.Flight;

// Per-booking record from the registry: boarding proof + owning identity + baggage status.
public record BookingStatusResponse(
    string FlightNumber,
    string AirlineName,
    string? ArrivalDate,
    string? ArrivalAirport,
    string IdentityNumber,
    bool IsBaggageDelayed,
    double BaggageDelayHours
);
