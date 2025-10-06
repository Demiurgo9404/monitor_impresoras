using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Services;
using MonitorImpresoras.Infrastructure.Repositories;

namespace MonitorImpresoras.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Configurar DbContext básico
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=MonitorImpresoras;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Registrar servicios básicos que compilan
        services.AddScoped<IPrinterRepository, PrinterRepository>();
        services.AddScoped<IWindowsPrinterService, WindowsPrinterService>();
        services.AddScoped<ISnmpService, SnmpService>();
        services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();

        // Servicios opcionales
        try
        {
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
        }
        catch
        {
            // Si hay errores, continuar sin estos servicios
        }

        return services;
    }
}
