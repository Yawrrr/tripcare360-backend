using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Admin;

namespace Tripcare360.Infrastructure.Persistence;

public class DbSeeder(IAdminUserRepository repo, IPasswordHasher hasher)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await repo.AnyAsync(ct)) return;

        await repo.AddAsync(new AdminUserEntity
        {
            Id           = Guid.NewGuid(),
            Email        = "admin@tripcare360.com",
            PasswordHash = hasher.Hash("Admin@123456"),
            Role         = "ClaimsOfficer",
            IsActive     = true,
        }, ct);
    }
}
