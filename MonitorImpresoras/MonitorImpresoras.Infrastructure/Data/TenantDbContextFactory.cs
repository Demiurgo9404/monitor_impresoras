using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Infrastructure.Data
{
    /// <summary>
    /// Fábrica de contextos de base de datos por tenant
    /// </summary>
    public class TenantDbContextFactory : ITenantDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TenantDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ITenantDbContext> CreateForTenantAsync(TenantInfo tenant)
        {
            // Crear un nuevo contexto de base de datos con la connection string del tenant
            var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            optionsBuilder.UseNpgsql(tenant.ConnectionString);

            var dbContext = new TenantContext(optionsBuilder.Options);

            // Asegurar que la base de datos existe
            await dbContext.Database.EnsureCreatedAsync();

            return dbContext;
        }
    }
}
