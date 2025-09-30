using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Options;
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

        // Registro mínimo de infraestructura para baseline
        // Evitamos registrar servicios excluidos temporalmente del compilado
        // Registrar el factory para el DbContext
        services.AddScoped<ApplicationDbContextFactory>();

        // Configurar el logger
        services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Opciones de monitoreo omitidas temporalmente para baseline
        
        return services;
    }
}
