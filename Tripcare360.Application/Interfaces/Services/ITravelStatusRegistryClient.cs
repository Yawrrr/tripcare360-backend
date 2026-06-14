using Tripcare360.Application.Dtos.Flight;

namespace Tripcare360.Application.Interfaces.Services;

public interface ITravelStatusRegistryClient
{
    // Flight-level status (on-blur auto-fill).
    Task<FlightStatusResponse?> GetFlightStatusAsync(string flightNumber);

    // Per-booking record (boarding proof + identity + baggage) used at claim submit.
    Task<BookingStatusResponse?> GetBookingAsync(string flightNumber, string bookingNumber);
}
