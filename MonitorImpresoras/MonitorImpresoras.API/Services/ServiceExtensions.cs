using Microsoft.Extensions.DependencyInjection;
using MonitorImpresoras.Application;
using MonitorImpresoras.Infrastructure;

namespace MonitorImpresoras.API.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // TODO: Register Application services here
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructure(configuration);
            return services;
        }
    }
}
