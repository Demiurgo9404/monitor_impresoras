using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Servicio de background para ejecutar reportes programados automáticamente
    /// </summary>
    public class ReportSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReportSchedulerService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Verificar cada 5 minutos

        public ReportSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<ReportSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Report Scheduler Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledReports();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scheduled reports");
                }

                // Esperar antes de la siguiente verificación
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Report Scheduler Service stopped");
        }

        private async Task ProcessScheduledReports()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scheduledReportService = scope.ServiceProvider.GetRequiredService<IScheduledReportService>();

                // Obtener reportes que deben ejecutarse
                var reportsToExecute = await scheduledReportService.GetReportsToExecuteAsync();

                if (reportsToExecute.Any())
                {
                    _logger.LogInformation("Found {Count} scheduled reports to execute", reportsToExecute.Count);

                    foreach (var scheduledReport in reportsToExecute)
                    {
                        try
                        {
                            _logger.LogInformation("Executing scheduled report: {ScheduledReportId} - {Name}", 
                                scheduledReport.Id, scheduledReport.Name);

                            var success = await scheduledReportService.ExecuteScheduledReportAsync(scheduledReport.Id);

                            if (success)
                            {
                                _logger.LogInformation("Scheduled report executed successfully: {ScheduledReportId}", 
                                    scheduledReport.Id);
                            }
                            else
                            {
                                _logger.LogWarning("Scheduled report execution failed: {ScheduledReportId}", 
                                    scheduledReport.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing scheduled report {ScheduledReportId}", 
                                scheduledReport.Id);
                        }

                        // Pequeña pausa entre ejecuciones para no sobrecargar el sistema
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                }
                else
                {
                    _logger.LogDebug("No scheduled reports to execute at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessScheduledReports");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Report Scheduler Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
