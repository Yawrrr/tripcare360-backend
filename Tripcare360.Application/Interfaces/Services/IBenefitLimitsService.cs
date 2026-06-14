using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Interfaces.Services;

public interface IBenefitLimitsService
{
    decimal GetMaxPayout(TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge = 0, string country = "");

    (decimal RatePerBlock, decimal BlockSizeHours, decimal MaxPayout) GetFlightDelayRate(TravelRoute route, PolicyTier tier, string country = "");

    (decimal DailyRate, int MaxDays, decimal MaxPayout) GetConfinementRate(TravelRoute route, PolicyTier tier, string country = "");

    (decimal DailyRate, int MaxDays, decimal MaxPayout) GetHijackRate(TravelRoute route, PolicyTier tier, string country = "");

    IReadOnlyList<BenefitItemDto> GetAllBenefits(TravelRoute route, PolicyTier tier, int insuredAge = 0, string country = "");
}
