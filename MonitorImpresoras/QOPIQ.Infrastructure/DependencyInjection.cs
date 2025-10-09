using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QOPIQ.Application.Interfaces;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Services;
using QOPIQ.Infrastructure.Repositories;

namespace QOPIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Configurar DbContext básico
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=QOPIQ;Username=postgres;Password=postgres";

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

