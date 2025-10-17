using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Data.Repositories;
using QOPIQ.Infrastructure.Services;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using QOPIQ.Infrastructure.Configuration;

// Use fully qualified names to avoid ambiguity
using IPrinterRepository = QOPIQ.Domain.Interfaces.Repositories.IPrinterRepository;
using ISnmpService = QOPIQ.Domain.Interfaces.Services.ISnmpService;
using IPrinterMonitoringService = QOPIQ.Application.Interfaces.IPrinterMonitoringService;
using IUnitOfWork = QOPIQ.Domain.Interfaces.IUnitOfWork;
using IAuthService = QOPIQ.Application.Interfaces.IAuthService;

namespace QOPIQ.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // Add database context
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            // Repositories - Use Domain interfaces with Infrastructure implementations
            // Register specific repository implementations for each entity type
            services.AddScoped<Domain.Interfaces.Repositories.IUserRepository, UserRepository>();
            services.AddScoped<IPrinterRepository, PrinterRepository>();
            services.AddScoped<Domain.Interfaces.Repositories.ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<Domain.Interfaces.Repositories.IRefreshTokenRepository, RefreshTokenRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, Data.UnitOfWork>();

            // Register SNMP options from configuration
            services.Configure<SnmpOptions>(configuration.GetSection("Snmp"));
            
            // Register SNMP service
            services.AddScoped<ISnmpService, SnmpService>();
            
            // Register PrinterMonitoringService with all dependencies
            services.AddScoped<IPrinterMonitoringService>(provider =>
            {
                var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
                var snmpService = provider.GetRequiredService<ISnmpService>();
                var options = provider.GetRequiredService<IOptions<SnmpOptions>>();
                
                return new PrinterMonitoringService(
                    unitOfWork,
                    snmpService,
                    options
                );
            });
            
            // Register other services if they exist in the Application layer
            // Commented out until PrinterService dependencies are resolved
            // var printerServiceType = Type.GetType("QOPIQ.Application.Interfaces.IPrinterService, QOPIQ.Application");
            // if (printerServiceType != null)
            // {
            //     var printerServiceImpl = Type.GetType("QOPIQ.Infrastructure.Services.PrinterService, QOPIQ.Infrastructure");
            //     if (printerServiceImpl != null)
            //     {
            //         services.AddScoped(printerServiceType, printerServiceImpl);
            //     }
            // }

            // Configuración de JWT
            services.Configure<QOPIQ.Domain.Settings.JwtSettings>(
                configuration.GetSection("JwtSettings"));

            // Add HttpContextAccessor for current user service
            services.AddHttpContextAccessor();
            services.AddScoped<Application.Interfaces.ICurrentUserService, Services.CurrentUserService>();

            // Register AuthService with all its dependencies
            services.AddScoped<IAuthService, AuthService>();
            
            // Add logging
            services.AddLogging();

            // BCrypt.Net-Next is used statically, no DI registration needed

            return services;
        }
    }
}
