using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces;
using QOPIQ.Application.Interfaces.MultiTenancy;
using QOPIQ.Domain.Common;
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
        try
        {
            // Configurar DbContext
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Host=localhost;Database=QOPIQ;Username=postgres;Password=postgres";

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString);
                
                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Configuración básica de servicios
            services.AddHttpContextAccessor();
            
            // Registrar servicios de multi-tenancy
            services.AddScoped<ITenantAccessor, TenantAccessor>();
            services.AddScoped<ITenantResolver, TenantResolver>();
            
            // Registrar servicios de repositorios
            services.AddScoped<IPrinterRepository, PrinterRepository>();
            services.AddScoped<IUnitOfWork, Data.UnitOfWork>();
            
            // Registrar servicios de aplicación
            services.AddScoped<IPrinterService, PrinterService>();
            services.AddScoped<ISnmpService, SnmpService>();
            
            // Configurar middleware de resolución de tenant
            services.AddTransient<TenantResolutionMiddleware>();
            
            return services;
        }
        catch (Exception ex)
        {
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<DependencyInjection>>();
            logger.LogError(ex, "Error al configurar los servicios de infraestructura");
            throw;
        }
    }
}

