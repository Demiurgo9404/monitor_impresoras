using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.Infrastructure.Data.Context
{
    public class TenantContext : DbContext, ITenantDbContext
    {
        public TenantContext(DbContextOptions<TenantContext> options) : base(options) { }

        // DbSets para las entidades del tenant
        public DbSet<User> Users { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<PrinterConsumablePart> PrinterConsumableParts { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<ScheduledReport> ScheduledReports { get; set; }
        public DbSet<ReportExecution> ReportExecutions { get; set; }
        public DbSet<AlertRule> AlertRules { get; set; }
    }
}
