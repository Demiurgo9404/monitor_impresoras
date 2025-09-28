using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
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
        }
    }
}
