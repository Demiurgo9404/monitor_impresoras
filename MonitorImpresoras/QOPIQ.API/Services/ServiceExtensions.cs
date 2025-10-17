using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using QOPIQ.Application;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Infrastructure;
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

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Registrar servicios de infraestructura
            services.AddInfrastructureServices(configuration, environment);
            
            // Registrar servicios personalizados
            services.AddScoped<QOPIQ.Domain.Interfaces.Services.ISnmpService, QOPIQ.Infrastructure.Services.SnmpService>();
            services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();
            
            return services;
        }
    }
}

