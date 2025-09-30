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
        public DbSet<UserClaim> UserClaims => Set<UserClaim>();
        public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
        public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();
        public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();
        // Nuevas entidades agregadas
        public DbSet<SystemEvent> SystemEvents { get; set; } = default!;
        public DbSet<NotificationEscalationHistory> NotificationEscalationHistory { get; set; } = default!;
        public DbSet<PrinterTelemetry> PrinterTelemetry { get; set; } = default!;
        public DbSet<PrinterTelemetryClean> PrinterTelemetryClean { get; set; } = default!;
        public DbSet<MaintenancePrediction> MaintenancePredictions { get; set; } = default!;
        public DbSet<PredictionFeedback> PredictionFeedback { get; set; } = default!;
        public DbSet<PredictionTrainingData> PredictionTrainingData { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración de filtros globales para multi-tenant
            ConfigureMultiTenantFilters(builder);

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
            });

            // Relación con User (si se requiere) puede configurarse aquí
            // Configuración de UserClaim
            builder.Entity<UserClaim>(b =>
            {
                b.ToTable("UserClaims");
                b.HasKey(uc => uc.Id);

                b.Property(uc => uc.ClaimType).IsRequired().HasMaxLength(200);
                b.Property(uc => uc.ClaimValue).IsRequired().HasMaxLength(200);
                b.Property(uc => uc.Description).HasMaxLength(500);
                b.Property(uc => uc.Category).HasMaxLength(100);
                b.Property(uc => uc.CreatedAtUtc).IsRequired();
                b.Property(uc => uc.UpdatedAtUtc);
                b.Property(uc => uc.ExpiresAtUtc);
                b.Property(uc => uc.CreatedByUserId).HasMaxLength(450);
                b.Property(uc => uc.UpdatedByUserId).HasMaxLength(450);

                // Índices para búsquedas rápidas
                b.HasIndex(uc => new { uc.UserId, uc.ClaimType }).IsUnique();
                b.HasIndex(uc => uc.ClaimType);
                b.HasIndex(uc => uc.Category);
                b.HasIndex(uc => uc.IsActive);
                b.HasIndex(uc => uc.ExpiresAtUtc);
            });

            // Relación con User (si se requiere) puede configurarse aquí
            // Configuración de ScheduledReport
            builder.Entity<ScheduledReport>(b =>
            {
                b.ToTable("ScheduledReports");
                b.HasKey(sr => sr.Id);

                b.Property(sr => sr.Name).IsRequired().HasMaxLength(200);
                b.Property(sr => sr.Description).HasMaxLength(500);
                b.Property(sr => sr.CronExpression).IsRequired().HasMaxLength(100);
                b.Property(sr => sr.Format).IsRequired().HasMaxLength(20);
                b.Property(sr => sr.Recipients).HasMaxLength(1000);
                b.Property(sr => sr.FixedParameters).HasColumnType("jsonb");

                // Índices para búsquedas rápidas
                b.HasIndex(sr => sr.ReportTemplateId);
                b.HasIndex(sr => sr.CreatedByUserId);
                b.HasIndex(sr => sr.IsActive);
                b.HasIndex(sr => sr.NextExecutionUtc);

                // Relaciones
                b.HasOne(sr => sr.ReportTemplate)
                    .WithMany(rt => rt.ScheduledReports)
                    .HasForeignKey(sr => sr.ReportTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(sr => sr.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(sr => sr.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de SystemEvent
            builder.Entity<SystemEvent>(b =>
            {
                b.ToTable("SystemEvents");
                b.HasKey(se => se.Id);

                b.Property(se => se.EventType).IsRequired().HasMaxLength(100);
                b.Property(se => se.Category).IsRequired().HasMaxLength(50);
                b.Property(se => se.Severity).IsRequired().HasMaxLength(20);
                b.Property(se => se.Title).IsRequired().HasMaxLength(200);
                b.Property(se => se.Description).HasMaxLength(2000);
                b.Property(se => se.EventData).HasColumnType("jsonb");
                b.Property(se => se.UserId).HasMaxLength(450);
                b.Property(se => se.IpAddress).HasMaxLength(45);
                b.Property(se => se.UserAgent).HasMaxLength(500);
                b.Property(se => se.SessionId).HasMaxLength(100);
                b.Property(se => se.RequestId).HasMaxLength(100);
                b.Property(se => se.Endpoint).HasMaxLength(500);
                b.Property(se => se.HttpMethod).HasMaxLength(10);
                b.Property(se => se.ErrorMessage).HasMaxLength(1000);
                b.Property(se => se.StackTrace).HasMaxLength(4000);
                b.Property(se => se.EnvironmentInfo).HasMaxLength(1000);
                b.Property(se => se.ApplicationVersion).HasMaxLength(50);
                b.Property(se => se.ServerName).HasMaxLength(100);

                // Índices para búsquedas rápidas
                b.HasIndex(se => se.EventType);
                b.HasIndex(se => se.Category);
                b.HasIndex(se => se.Severity);
                b.HasIndex(se => se.UserId);
                b.HasIndex(se => se.TimestampUtc);
                b.HasIndex(se => se.IsSuccess);
                b.HasIndex(se => new { se.Category, se.Severity, se.TimestampUtc });

                // Relación con User
                b.HasOne(se => se.User)
                    .WithMany()
                    .HasForeignKey(se => se.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de PredictionTrainingData
            builder.Entity<PredictionTrainingData>(b =>
            {
                b.ToTable("PredictionTrainingData");
                b.HasKey(ptd => ptd.Id);

                b.Property(ptd => ptd.FeedbackId).IsRequired();
                b.Property(ptd => ptd.PredictionId).IsRequired();
                b.Property(ptd => ptd.InputData).IsRequired();
                b.Property(ptd => ptd.OriginalProbability).IsRequired().HasColumnType("decimal(5,4)");
                b.Property(ptd => ptd.CorrectedProbability).HasColumnType("decimal(5,4)");
                b.Property(ptd => ptd.ActualEventDate);
                b.Property(ptd => ptd.ActualDaysUntilEvent);
                b.Property(ptd => ptd.PredictionType).IsRequired().HasMaxLength(50);
                b.Property(ptd => ptd.TrainingWeight).HasColumnType("decimal(3,2)").HasDefaultValue(1.0m);
                b.Property(ptd => ptd.CreatedAt).IsRequired();

                // Índices para consultas frecuentes
                b.HasIndex(ptd => ptd.FeedbackId);
                b.HasIndex(ptd => ptd.PredictionId);
                b.HasIndex(ptd => ptd.PredictionType);
                b.HasIndex(ptd => ptd.IsReadyForTraining);
                b.HasIndex(ptd => new { ptd.PredictionType, ptd.IsReadyForTraining });
                b.HasIndex(ptd => ptd.CreatedAt);
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

        /// <summary>
        /// Configura filtros globales para aislamiento multi-tenant
        /// </summary>
        private void ConfigureMultiTenantFilters(ModelBuilder builder)
        {
            // Nota: Estos filtros requieren que el contexto tenga acceso al TenantId actual
            // En producción, esto se haría mediante un servicio de contexto de tenant

            // Ejemplo de configuración (se activaría cuando se implemente el contexto de tenant):
            /*
            builder.Entity<Printer>().HasQueryFilter(p => p.TenantId == _currentTenantService.TenantId);
            builder.Entity<PrinterTelemetry>().HasQueryFilter(t => t.TenantId == _currentTenantService.TenantId);
            builder.Entity<MaintenancePrediction>().HasQueryFilter(p => p.TenantId == _currentTenantService.TenantId);
            builder.Entity<PredictionFeedback>().HasQueryFilter(f => f.TenantId == _currentTenantService.TenantId);
            */

            // Por ahora, comentar estos filtros hasta implementar el servicio de contexto de tenant
        }

    }
}
