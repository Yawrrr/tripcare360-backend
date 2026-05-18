namespace Tripcare360.Application.Interfaces.Services;

public interface IRojakkkService
{
    Task<bool> VerifyFlightDelayAsync(string flightNumber, DateTime date);
}
