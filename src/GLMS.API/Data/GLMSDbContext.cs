using GLMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Data
{
    public class GLMSDbContext : DbContext
    {
        public GLMSDbContext(DbContextOptions<GLMSDbContext> options) : base(options) { }

        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Contract configurations
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                entity.HasIndex(e => e.ClientEmail);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.EndDate);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.ContractValue).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.Type).HasConversion<string>();
            });

            // ServiceRequest configurations
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasIndex(e => e.RequestNumber).IsUnique();
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequestDate);

                entity.Property(e => e.EstimatedCost).HasPrecision(18, 2);
                entity.Property(e => e.ActualCost).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.Type).HasConversion<string>();
            });

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasMaxLength(50);
            });
        }
    }
}