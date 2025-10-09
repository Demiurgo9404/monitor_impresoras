using Microsoft.Extensions.DependencyInjection;
using QOPIQ.Application;
using QOPIQ.Infrastructure;

namespace QOPIQ.API.Services
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

