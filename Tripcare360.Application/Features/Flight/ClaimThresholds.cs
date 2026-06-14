using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Flight;

// Minimum automated-verification thresholds, shared by the on-blur verify queries
// and the reserve command so the eligibility decision is computed identically.
public static class ClaimThresholds
{
    public const double FlightDelayMinHours = 2;
    public const double BaggageDelayMinHours = 6;

    // Returns true when the flight's domestic/international character matches the policy route.
    public static bool IsFlightRouteMatch(string? depCountry, string? arrCountry, TravelRoute policyRoute)
    {
        bool isDomesticFlight = string.Equals(depCountry, arrCountry, StringComparison.OrdinalIgnoreCase);
        return policyRoute == TravelRoute.Domestic ? isDomesticFlight : !isDomesticFlight;
    }
}
