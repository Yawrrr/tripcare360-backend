using Microsoft.EntityFrameworkCore;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Enums;
using Tripcare360.Infrastructure.Persistence;

namespace Tripcare360.Infrastructure.Repositories;

public class ClaimRepository(Tripcare360DbContext db)
    : GenericRepository<ClaimEntity>(db), IClaimRepository
{
    public async Task<ClaimEntity?> GetByClaimCodeAsync(string claimCode) =>
        await Db.Claims.FirstOrDefaultAsync(c => c.ClaimCode == claimCode);

    public async Task<IReadOnlyList<ClaimEntity>> GetExpiredPendingAsync(
        DateTimeOffset cutoff, CancellationToken ct = default) =>
        await Db.Claims
            .Where(c => c.Status == ClaimStatus.Pending && c.CreatedAt < cutoff)
            .ToListAsync(ct);
}
