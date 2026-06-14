using Microsoft.EntityFrameworkCore;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Domain.Entities.Admin;
using Tripcare360.Infrastructure.Persistence;

namespace Tripcare360.Infrastructure.Repositories;

public class AdminUserRepository(Tripcare360DbContext db)
    : GenericRepository<AdminUserEntity>(db), IAdminUserRepository
{
    public async Task<AdminUserEntity?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Db.Set<AdminUserEntity>()
               .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> AnyAsync(CancellationToken ct = default)
        => await Db.Set<AdminUserEntity>().AnyAsync(ct);
}
