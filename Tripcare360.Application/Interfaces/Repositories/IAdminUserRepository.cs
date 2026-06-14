using Tripcare360.Domain.Entities.Admin;

namespace Tripcare360.Application.Interfaces.Repositories;

public interface IAdminUserRepository : IGenericRepository<AdminUserEntity>
{
    Task<AdminUserEntity?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
}
