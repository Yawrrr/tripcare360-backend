using Tripcare360.Domain.Entities.Claim;

namespace Tripcare360.Application.Interfaces.Repositories;

public interface IClaimRepository : IGenericRepository<ClaimEntity>
{
    Task<ClaimEntity?> GetByClaimCodeAsync(string claimCode);
    Task<IReadOnlyList<ClaimEntity>> GetExpiredPendingAsync(DateTimeOffset cutoff, CancellationToken ct = default);
}
