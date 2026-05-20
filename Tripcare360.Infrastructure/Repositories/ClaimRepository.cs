using Microsoft.EntityFrameworkCore;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Infrastructure.Persistence;

namespace Tripcare360.Infrastructure.Repositories;

public class ClaimRepository(Tripcare360DbContext db)
    : GenericRepository<ClaimEntity>(db), IClaimRepository
{
    public async Task<ClaimEntity?> GetByClaimCodeAsync(string claimCode) =>
        await Db.Claims.FirstOrDefaultAsync(c => c.ClaimCode == claimCode);
}
