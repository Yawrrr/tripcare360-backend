using Tripcare360.Domain.Entities.Claim;

namespace Tripcare360.Application.Interfaces.Repositories;

public interface IClaimRepository : IGenericRepository<ClaimEntity>
{
    // Claim-specific query methods go here
}
