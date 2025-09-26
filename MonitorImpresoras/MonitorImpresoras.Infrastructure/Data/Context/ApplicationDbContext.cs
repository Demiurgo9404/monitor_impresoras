using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;
using System.Reflection;

namespace MonitorImpresoras.Infrastructure.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string,
        IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Printer> Printers { get; set; }
        public DbSet<PrinterConsumable> PrinterConsumables { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure the many-to-many relationship between User and Role
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            // Configure JSON serialization for FilterParameters in Report
            modelBuilder.Entity<Report>()
                .Property(r => r.FilterParameters)
                .HasConversion(
                    v => v ?? string.Empty,
                    v => string.IsNullOrEmpty(v) ? null : v
                );
        }
    }
}
