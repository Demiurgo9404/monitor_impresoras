using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QOPIQ.Application;
using QOPIQ.Domain.Interfaces.Services;
using QOPIQ.Infrastructure;
using QOPIQ.Infrastructure.Configuration;
using QOPIQ.Infrastructure.Services;

namespace QOPIQ.API.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // TODO: Register Application services here
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            // Registrar servicios de infraestructura
            services.AddInfrastructureServices(configuration, environment);
            
            // Registrar servicios personalizados
            services.AddScoped<ISnmpService, SnmpService>();
            services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();
            
            return services;
        }
    }
}

