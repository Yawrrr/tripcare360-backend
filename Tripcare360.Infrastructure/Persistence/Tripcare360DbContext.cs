using Microsoft.EntityFrameworkCore;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Entities.Common;

namespace Tripcare360.Infrastructure.Persistence
{
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

            modelBuilder.Entity<ClaimEntity>()
                .Property(c => c.Type)
                .HasConversion<string>();

            modelBuilder.Entity<ClaimEntity>()
                .Property(c => c.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ClaimEntity>()
                .Property(c => c.EstimatedPayout)
                .HasPrecision(18, 2);

        }
    }
}
