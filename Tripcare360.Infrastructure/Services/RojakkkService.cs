using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Services;

public class RojakkkService : IRojakkkService
{
    public async Task<bool> VerifyFlightDelayAsync(string flightNumber, DateTime date)
    {
        await Task.Delay(1500);
        return flightNumber.ToUpper() == "MH123";
    }
}
