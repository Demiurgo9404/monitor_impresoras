using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Interfaces;
using MonitorImpresoras.Domain.Interfaces.Services;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Data.Context;
using MonitorImpresoras.Infrastructure.Data.Repositories;
using MonitorImpresoras.Infrastructure.Services;
using MonitorImpresoras.Infrastructure.Services.Monitoring;

namespace MonitorImpresoras.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext with PostgreSQL
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Register repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPrinterRepository, PrinterRepository>();
            services.AddScoped<IPrintJobRepository, PrintJobRepository>();
            services.AddScoped<IConsumableRepository, ConsumableRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            // Add other repositories here as they are created

            // Register services
            services.AddScoped<IPrinterService, PrinterService>();
            services.AddScoped<IConsumableService, ConsumableService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IPrintJobService, PrintJobService>();
            services.AddScoped<ISnmpService, SnmpService>();

            // Register monitoring services
            services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();
            services.AddSingleton<IMonitoringService, MonitoringService>();
            services.AddHostedService<PrinterMonitoringBackgroundService>();

            return services;
        }
    }
}
