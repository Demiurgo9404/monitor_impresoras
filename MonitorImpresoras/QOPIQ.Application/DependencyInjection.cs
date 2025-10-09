using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace QOPIQ.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuración mínima para que compile
        // Los servicios específicos se agregarán después
        
        return services;
    }
}

