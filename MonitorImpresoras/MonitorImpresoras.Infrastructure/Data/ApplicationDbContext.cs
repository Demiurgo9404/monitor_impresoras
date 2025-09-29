using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;
using System;
using System.Linq;

namespace MonitorImpresoras.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Entidades de dominio
        public DbSet<Printer> Printers => Set<Printer>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración de RefreshToken
            builder.Entity<RefreshToken>(b =>
            {
                b.ToTable("RefreshTokens");
                b.HasKey(rt => rt.Id);

                b.Property(rt => rt.Token).IsRequired().HasMaxLength(100);
                b.Property(rt => rt.UserId).IsRequired().HasMaxLength(450);
                b.Property(rt => rt.ExpiresAtUtc).IsRequired();
                b.Property(rt => rt.CreatedAtUtc).IsRequired();
                b.Property(rt => rt.CreatedByIp).HasMaxLength(45);
                b.Property(rt => rt.RevokedByIp).HasMaxLength(45);
                b.Property(rt => rt.ReplacedByToken).HasMaxLength(100);

                // Índices para búsquedas rápidas
                b.HasIndex(rt => rt.Token).IsUnique();
                b.HasIndex(rt => new { rt.UserId, rt.IsActive })
                    .HasFilter("\"Revoked\" = false AND \"ExpiresAtUtc\" > NOW()");

                // Relación con User
                b.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
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

            // Configuración de Printer
            builder.Entity<Printer>(b =>
            {
                b.ToTable("Printers");
                b.HasKey(p => p.Id);

                b.Property(p => p.Name).IsRequired().HasMaxLength(100);
                b.Property(p => p.Model).IsRequired().HasMaxLength(100);
                b.Property(p => p.SerialNumber).IsRequired().HasMaxLength(100);
                b.Property(p => p.IpAddress).IsRequired().HasMaxLength(50);
                b.Property(p => p.Location).HasMaxLength(200);
                b.Property(p => p.Status).HasMaxLength(50);
                b.Property(p => p.CommunityString).HasMaxLength(50);
                b.Property(p => p.Notes).HasMaxLength(500);
                b.Property(p => p.LastError).HasMaxLength(1000);

                // Índices para búsquedas rápidas
                b.HasIndex(p => p.SerialNumber).IsUnique();
                b.HasIndex(p => p.IpAddress);
                b.HasIndex(p => p.Status);
                b.HasIndex(p => p.IsOnline);
            });

            // Configuración de AuditLog
            builder.Entity<AuditLog>(b =>
            {
                b.ToTable("AuditLogs");
                b.HasKey(a => a.Id);

                b.Property(a => a.UserId).IsRequired().HasMaxLength(100);
                b.Property(a => a.Action).IsRequired().HasMaxLength(100);
                b.Property(a => a.Entity).IsRequired().HasMaxLength(100);
                b.Property(a => a.EntityId).HasMaxLength(100);
                b.Property(a => a.Details).HasMaxLength(500);
                b.Property(a => a.IpAddress).HasMaxLength(50);
                b.Property(a => a.UserAgent).HasMaxLength(500);
                b.Property(a => a.Timestamp).IsRequired();

                // Índices para búsquedas rápidas
                b.HasIndex(a => a.UserId);
                b.HasIndex(a => a.Action);
                b.HasIndex(a => a.Entity);
                b.HasIndex(a => a.Timestamp);
                b.HasIndex(a => new { a.Entity, a.EntityId });
                b.HasIndex(a => new { a.UserId, a.Timestamp });

                b.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuraciones adicionales para otras entidades...
        }
    }
}
