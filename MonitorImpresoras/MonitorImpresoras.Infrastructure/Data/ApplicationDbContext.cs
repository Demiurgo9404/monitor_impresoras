using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorImpresoras.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>,
        UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Entidades de dominio
        public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
        public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
        // Agrega otros DbSet según sea necesario

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración de User
            builder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasIndex(u => u.UserName).IsUnique();
                b.HasIndex(u => u.Email).IsUnique();
                b.HasIndex(u => u.NormalizedUserName).IsUnique();
                b.HasIndex(u => u.NormalizedEmail).IsUnique();
                b.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                b.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                b.Property(u => u.RefreshToken).HasMaxLength(500);
            });

            // Configuración de Role
            builder.Entity<Role>(b =>
            {
                b.ToTable("Roles");
                b.HasIndex(r => r.NormalizedName).IsUnique();
                b.Property(r => r.Description).HasMaxLength(500);
            });

            // Configuración de UserRole
            builder.Entity<UserRole>(b =>
            {
                b.ToTable("UserRoles");
                b.HasKey(ur => new { ur.UserId, ur.RoleId });

                b.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                b.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            // Configuración de LoginAttempt
            builder.Entity<LoginAttempt>(b =>
            {
                b.ToTable("LoginAttempts");
                b.HasKey(x => x.Id);

                b.Property(x => x.AttemptDate).IsRequired();
                b.Property(x => x.IpAddress).HasMaxLength(50);
                b.Property(x => x.UserAgent).HasMaxLength(1000);
                b.Property(x => x.FailureReason).HasMaxLength(1000);

                // Índices para búsquedas rápidas
                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.Username);
                b.HasIndex(x => x.AttemptDate);
                b.HasIndex(x => x.Success);
                b.HasIndex(x => new { x.Username, x.AttemptDate });
                b.HasIndex(x => new { x.IpAddress, x.AttemptDate });

                b.HasOne(x => x.User)
                    .WithMany(u => u.LoginAttempts)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuraciones adicionales para otras entidades...
        }
    }
}
