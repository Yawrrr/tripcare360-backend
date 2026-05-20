using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Entities.Common;

namespace Tripcare360.Infrastructure.Persistence;

public class Tripcare360DbContext : DbContext
{
    public Tripcare360DbContext(DbContextOptions<Tripcare360DbContext> options) : base(options) { }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<ClaimEntity> Claims => Set<ClaimEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ClaimEntity>(entity =>
        {
            entity.Property(c => c.Type).HasConversion<string>();
            entity.Property(c => c.Status).HasConversion<string>();
            entity.Property(c => c.Route).HasConversion<string>();
            entity.Property(c => c.Tier).HasConversion<string>();
            entity.Property(c => c.SubmittedAmount).HasPrecision(18, 2);
            entity.Property(c => c.CalculatedPayout).HasPrecision(18, 2);

            // FileObjectKeys stored as JSON array in a single column
            entity.Property(c => c.FileObjectKeys)
                .HasConversion(
                    keys => System.Text.Json.JsonSerializer.Serialize(keys, (System.Text.Json.JsonSerializerOptions?)null),
                    json => System.Text.Json.JsonSerializer.Deserialize<List<string>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (a, b) => a != null && b != null && a.SequenceEqual(b),
                    v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                    v => v.ToList()));
        });
    }
}
