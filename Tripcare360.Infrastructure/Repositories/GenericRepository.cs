using Microsoft.EntityFrameworkCore;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Infrastructure.Persistence;

namespace Tripcare360.Infrastructure.Repositories;

public class GenericRepository<T>(Tripcare360DbContext db) : IGenericRepository<T> where T : class
{
    protected readonly Tripcare360DbContext Db = db;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Db.Set<T>().FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Db.Set<T>().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        Db.Set<T>().Add(entity);
        await Db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        Db.Set<T>().Update(entity);
        await Db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        Db.Set<T>().Remove(entity);
        await Db.SaveChangesAsync(ct);
    }
}
