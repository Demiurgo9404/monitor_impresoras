using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QOPIQ.Application.Interfaces;
using QOPIQ.Application.Interfaces.MultiTenancy;
using QOPIQ.Domain.Repositories;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Repositories;
using QOPIQ.Infrastructure.Services;
using QOPIQ.Infrastructure.Services.MultiTenancy;

namespace QOPIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Configurar DbContext con resolución de tenant
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=QOPIQ;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Registrar servicios de multi-tenancy
        services.AddHttpContextAccessor();
        
        // Registrar TenantAccessor
        services.AddScoped<ITenantAccessor, TenantAccessor>();
        
        // Registrar TenantResolver
        services.AddScoped<ITenantResolver, TenantResolver>();
        
        // Registrar middleware
        services.AddTransient<TenantResolutionMiddleware>();
        
        // Configurar opciones
        services.Configure<SnmpOptions>(configuration.GetSection("Snmp"));
        
        // Registrar servicios de repositorios
        services.AddScoped<IPrinterRepository>(sp => 
        {
            var context = sp.GetRequiredService<ApplicationDbContext>();
            var tenantAccessor = sp.GetRequiredService<ITenantAccessor>();
            var logger = sp.GetRequiredService<ILogger<PrinterRepository>>();
            return new PrinterRepository(context, tenantAccessor, logger);
        });
        
        // Registrar servicios de dominio
        services.AddScoped<IWindowsPrinterService, WindowsPrinterService>();
        services.AddScoped<ISnmpService, SnmpService>();
        
        // Registrar servicios de aplicación
        services.AddScoped<IPrinterMonitoringService>(sp => 
        {
            var repository = sp.GetRequiredService<IPrinterRepository>();
            var snmpService = sp.GetRequiredService<ISnmpService>();
            var logger = sp.GetRequiredService<ILogger<PrinterMonitoringService>>();
            var tenantAccessor = sp.GetRequiredService<ITenantAccessor>();
            
            return new PrinterMonitoringService(repository, snmpService, logger, tenantAccessor);
        });

        // Registrar servicios opcionales
        try
        {
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
        }
        catch (Exception ex)
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<DependencyInjection>>();
            logger?.LogWarning(ex, "No se pudieron registrar algunos servicios opcionales");
        }

        return services;
    }
}

