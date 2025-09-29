using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Workers
{
    /// <summary>
    /// Worker en background para ejecutar reportes programados
    /// </summary>
    public class ScheduledReportWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScheduledReportWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public ScheduledReportWorker(
            IServiceProvider serviceProvider,
            ILogger<ScheduledReportWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScheduledReportWorker iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledReportsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en ScheduledReportWorker");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("ScheduledReportWorker detenido");
        }

        private async Task ProcessScheduledReportsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var scheduledReportService = scope.ServiceProvider.GetRequiredService<IScheduledReportService>();

            var dueReports = await scheduledReportService.GetDueScheduledReportsAsync();

            foreach (var report in dueReports)
            {
                try
                {
                    _logger.LogInformation("Ejecutando reporte programado: {ScheduledReportId}", report.Id);

                    await scheduledReportService.ExecuteScheduledReportAsync(report, "system");

                    _logger.LogInformation("Reporte programado {ScheduledReportId} ejecutado exitosamente", report.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al ejecutar reporte programado: {ScheduledReportId}", report.Id);
                }
            }
        }
    }
}
