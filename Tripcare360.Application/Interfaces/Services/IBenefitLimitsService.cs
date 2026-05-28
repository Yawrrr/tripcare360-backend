using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Interfaces.Services;

public interface IBenefitLimitsService
{
    decimal GetMaxPayout(Country country, TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge = 0);
    (decimal RatePerBlock, decimal BlockSizeHours, decimal MaxPayout) GetFlightDelayRate(Country country, TravelRoute route, PolicyTier tier);
    (decimal DailyRate, int MaxDays, decimal MaxPayout) GetConfinementRate(Country country, TravelRoute route, PolicyTier tier);
    (decimal DailyRate, int MaxDays, decimal MaxPayout) GetHijackRate(Country country, TravelRoute route, PolicyTier tier);
    IReadOnlyList<BenefitItemDto> GetAllBenefits(Country country, TravelRoute route, PolicyTier tier, int insuredAge = 0);
}
