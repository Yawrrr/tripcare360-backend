using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Policy;

public record GetBenefitsRequest(TravelRoute Route, PolicyTier Tier, Country Country, int InsuredAge = 0);
