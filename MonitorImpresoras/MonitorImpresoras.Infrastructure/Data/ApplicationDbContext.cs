using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ITenantAccessor _tenantAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantAccessor tenantAccessor) : base(options)
        {
            _tenantAccessor = tenantAccessor;
        }

        // Entidades principales
        public DbSet<Printer> Printers { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<PrinterCounters> PrinterCounters { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<PrinterConsumable> PrinterConsumables { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantUser> TenantUsers { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<CostCalculationHistory> CostCalculationHistories { get; set; }
        public DbSet<MonitoringStats> MonitoringStats { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserQuota> UserQuotas { get; set; }
        public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar relaciones muchos a muchos
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Configurar filtros globales multi-tenant
            ConfigureTenantFilters(modelBuilder);
        }

        private void ConfigureTenantFilters(ModelBuilder modelBuilder)
        {
            // Filtro global para Printer
            modelBuilder.Entity<Printer>().HasQueryFilter(p => p.TenantId == _tenantAccessor.TenantId);
            
            // Filtro global para User
            modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantAccessor.TenantId);
            
            // Filtro global para Company
            modelBuilder.Entity<Company>().HasQueryFilter(c => c.TenantId == _tenantAccessor.TenantId);
            
            // Filtro global para Project
            modelBuilder.Entity<Project>().HasQueryFilter(p => p.TenantId == _tenantAccessor.TenantId);
            
            // Filtro global para Report
            modelBuilder.Entity<Report>().HasQueryFilter(r => r.TenantId == _tenantAccessor.TenantId);
            
            // Filtro global para PrintJob
            modelBuilder.Entity<PrintJob>().HasQueryFilter(pj => pj.TenantId == _tenantAccessor.TenantId);
            
            // Filtro global para Alert
            modelBuilder.Entity<Alert>().HasQueryFilter(a => a.TenantId == _tenantAccessor.TenantId);

            // Los Tenants NO tienen filtro (necesitamos acceso global para validación)
        }
    }
}
