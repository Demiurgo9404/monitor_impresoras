using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Printer> Printers { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<PrinterConsumable> Consumables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
