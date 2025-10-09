using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Infrastructure.Data
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
        
        // Entidades faltantes
        public DbSet<Project> Projects { get; set; }
        public DbSet<Company> Companies { get; set; }
        
        // Entidades enterprise
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<CostCalculationHistory> CostCalculationHistories { get; set; }
        public DbSet<MonitoringStats> MonitoringStats { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserQuota> UserQuotas { get; set; }
        public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }

        // Entidades de reportes
        public DbSet<ScheduledReport> ScheduledReports { get; set; }
        public DbSet<ReportExecution> ReportExecutions { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<ReportTemplate> ReportTemplates { get; set; }

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

            // Configurar relaciones enterprise
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Subscription)
                .WithMany(s => s.Invoices)
                .HasForeignKey(i => i.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar precisión decimal
            modelBuilder.Entity<Subscription>()
                .Property(s => s.MonthlyPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TaxAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasPrecision(10, 2);

            // Índices para mejor rendimiento
            modelBuilder.Entity<Subscription>()
                .HasIndex(s => new { s.UserId, s.Status })
                .HasDatabaseName("IX_Subscriptions_UserId_Status");

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.SubscriptionId, i.Status })
                .HasDatabaseName("IX_Invoices_SubscriptionId_Status");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.CompanyName)
                .HasDatabaseName("IX_Users_CompanyName");
        }
    }
}

