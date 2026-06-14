using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Policy;

public record GetBenefitsRequest(TravelRoute Route, PolicyTier Tier, int InsuredAge = 0, string Country = "");
