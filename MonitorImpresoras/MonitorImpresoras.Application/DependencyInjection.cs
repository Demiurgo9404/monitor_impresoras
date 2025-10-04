using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using AutoMapper;
using MonitorImpresoras.Application.Mappings;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Services;
using Microsoft.Extensions.Configuration;

namespace MonitorImpresoras.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            // Registrar AutoMapper con los perfiles de mapeo
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, Assembly.GetExecutingAssembly());
            
            // Registrar servicios de la capa de aplicación
            services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();
            
            return services;
        }
        catch (Exception ex)
        {
            var logger = services.BuildServiceProvider().GetService<ILogger>();
            logger?.LogError(ex, "Error al configurar los servicios de la aplicación");
            throw;
        }
    }
}
