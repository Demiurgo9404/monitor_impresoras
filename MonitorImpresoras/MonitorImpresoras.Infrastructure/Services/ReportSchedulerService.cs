using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Servicio de scheduler para reportes programados multi-tenant
    /// </summary>
    public class ReportSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<ReportSchedulerService> _logger;
        private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);

        public ReportSchedulerService(
            IServiceProvider provider,
            ILogger<ReportSchedulerService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReportSchedulerService iniciado - Procesando reportes programados por tenant");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
                    var tenants = await tenantService.GetAllTenantsAsync();

                    _logger.LogInformation("Procesando {TenantCount} tenants activos", tenants.Count());

                    foreach (var tenant in tenants.Where(t => t.IsActive &&
                        (t.ExpiresAt == null || t.ExpiresAt > DateTime.UtcNow)))
                    {
                        try
                        {
                            // Crear contexto específico del tenant
                            var dbFactory = scope.ServiceProvider.GetRequiredService<ITenantDbContextFactory>();
                            var tenantContext = await dbFactory.CreateForTenantAsync(tenant);

                            var scheduledService = scope.ServiceProvider.GetRequiredService<IScheduledReportService>();
                            var pendingReports = await scheduledService.GetPendingReportsAsync(tenantContext);

                            _logger.LogInformation("Tenant {TenantKey}: {ReportCount} reportes pendientes",
                                tenant.TenantKey, pendingReports.Count());

                            foreach (var report in pendingReports)
                            {
                                // Ejecutar en background seguro (fire-and-forget)
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        var execService = scope.ServiceProvider.GetRequiredService<IReportExecutionService>();
                                        var result = await execService.ExecuteReportAsync(tenantContext, report);

                                        var notificationService = scope.ServiceProvider.GetRequiredService<IAdvancedNotificationService>();
                                        await notificationService.SendReportAsync(tenantContext, result, report);

                                        await scheduledService.MarkExecutionCompletedAsync(tenantContext, report, result);

                                        _logger.LogInformation("Reporte ejecutado exitosamente: {ReportName} para tenant {TenantKey}",
                                            report.Name, tenant.TenantKey);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error ejecutando reporte {ReportId} para tenant {TenantKey}",
                                            report.Id, tenant.TenantKey);
                                    }
                                }, stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error procesando tenant {TenantKey}", tenant.TenantKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en ciclo principal del scheduler");
                }

                // Esperar siguiente ciclo
                await Task.Delay(_pollInterval, stoppingToken);
            }

            _logger.LogInformation("ReportSchedulerService detenido");
        }
    }
}
