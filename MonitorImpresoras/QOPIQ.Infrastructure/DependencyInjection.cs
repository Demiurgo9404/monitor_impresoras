using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Domain.Interfaces.Services;
using QOPIQ.Infrastructure.Configuration;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Repositories;
using QOPIQ.Infrastructure.Services;

namespace QOPIQ.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Configuración de opciones
            services.Configure<SnmpOptions>(configuration.GetSection("Snmp"));

            // Base de datos
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Repositorios genéricos
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Repositorios específicos
            services.AddScoped<IPrinterRepository, PrinterRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Servicios
            services.AddScoped<IPrinterRepository, PrinterRepository>();
            
            // Servicios
            services.AddScoped<ISnmpService, SnmpService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Configuración para desarrollo
            if (environment.IsDevelopment())
            {
                // Configuraciones específicas para desarrollo
            }

            return services;
        }
    }
}

