using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QOPIQ.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=PrintHubDB;Username=postgres;Password=Roximar");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

