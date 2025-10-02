using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Interfaces.Services;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Services;
using MonitorImpresoras.Infrastructure.Services.SNMP;
using MonitorImpresoras.Infrastructure.Services.WMI;
using MonitorImpresoras.Infrastructure.Options;

namespace MonitorImpresoras.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Configurar DbContext con manejo de errores
        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
            }

            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                options.UseNpgsql(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    });

                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Registrar servicios de infraestructura
            services.AddScoped<IPrinterRepository, PrinterRepository>();
            services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();
            services.AddScoped<ISnmpService, SnmpService>();
            services.AddScoped<IWindowsPrinterService, WindowsPrinterService>();
            services.AddScoped<IPrinterStatusProvider, PrinterStatusProvider>();

            // Configuración de opciones
            services.Configure<PrinterMonitoringOptions>(
                configuration.GetSection("PrinterMonitoring"));

            // Configuración de HttpClient
            services.AddHttpClient();

            // Configuración de health checks
            services.AddHealthChecks()
                .AddCheck<PrinterHealthCheck>("printer_health_check");

            return services;
        }

        catch (Exception ex)
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<DependencyInjection>>();
            logger?.LogError(ex, "Error al configurar la base de datos");
            throw;
        }
    }
}
