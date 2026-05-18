using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tripcare360.Domain.Entities.Claim;

namespace Tripcare360.Infrastructure.Persistence
{
    public class Tripcare360DbContext : DbContext 
    {
        public Tripcare360DbContext(DbContextOptions<Tripcare360DbContext> options) : base(options) { }

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
