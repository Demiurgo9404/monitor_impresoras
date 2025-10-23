using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Printer> Printers { get; set; } = default!;
        public DbSet<Subscription> Subscriptions { get; set; } = default!;
        public DbSet<Invoice> Invoices { get; set; } = default!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<RolePermission> RolePermissions { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Printer>().ToTable("Printers");
            modelBuilder.Entity<Subscription>().ToTable("Subscriptions");
            modelBuilder.Entity<Invoice>().ToTable("Invoices");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");

            // Configurar relaciones
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(rp => rp.RoleId);
        }
    }
}
