using Tripcare360.Application.Dtos.Flight;

namespace Tripcare360.Application.Interfaces.Services;

public interface ITravelStatusRegistryClient
{
    Task<FlightStatusResponse?> GetFlightStatusAsync(string flightNumber, DateTime date);
}
