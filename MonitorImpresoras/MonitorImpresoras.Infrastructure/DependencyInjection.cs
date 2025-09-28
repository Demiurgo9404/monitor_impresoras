using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Services;
using System;

namespace MonitorImpresoras.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Registrar servicios de infraestructura con manejo de errores
        try
        {
            // Registrar servicios de aplicación
            services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();
            services.AddScoped<ISnmpService, SnmpService>();
            services.AddScoped<IWindowsPrinterService, WindowsPrinterService>();
            
            // Registrar el factory para el DbContext
            services.AddScoped<ApplicationDbContextFactory>();
            
            // Registrar el servicio de fondo para monitoreo de impresoras
            services.AddHostedService<PrinterMonitoringBackgroundService>();
            
            // Configurar el logger
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            // Configuración del servicio de monitoreo
            services.Configure<PrinterMonitoringOptions>(configuration.GetSection("PrinterMonitoring"));
        }
        catch (Exception ex)
        {
            // En un entorno real, deberías registrar este error en un sistema de registro
            Console.WriteLine($"Error al registrar servicios de infraestructura: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw; // Relanzar la excepción para que el sistema sepa que hubo un error
        }
        
        return services;
    }
}
