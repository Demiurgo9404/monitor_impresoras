using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using AutoMapper;
using MonitorImpresoras.Application.Mappings;
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
            
            // Registrar otros servicios de la capa de aplicación aquí
            // services.AddScoped<IMyService, MyService>();
            
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
