using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using Quartz;

namespace MonitorImpresoras.API.Services
{
    /// <summary>
    /// Job para envío automático de reportes diarios
    /// </summary>
    [DisallowConcurrentExecution]
    public class DailyReportJob : IJob
    {
        private readonly IAlertService _alertService;
        private readonly ILogger<DailyReportJob> _logger;
        private readonly IModelRetrainingService _retrainingService;

        public DailyReportJob(
            IAlertService alertService,
            ILogger<DailyReportJob> logger,
            IModelRetrainingService retrainingService)
        {
            _alertService = alertService;
            _logger = logger;
            _retrainingService = retrainingService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Iniciando ejecución de reporte diario programado");

                await _alertService.SendDailySummaryReportAsync();

                _logger.LogInformation("Reporte diario enviado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando reporte diario programado");
                throw; // Quartz manejará el reintento según configuración
            }
        }
    }
    /// <summary>
    /// Job para verificación periódica del estado de impresoras
    /// </summary>
    [DisallowConcurrentExecution]
    public class PrinterStatusCheckJob : IJob
    {
        private readonly IPrinterService _printerService;
        private readonly IAlertService _alertService;
        private readonly ILogger<PrinterStatusCheckJob> _logger;

        public PrinterStatusCheckJob(
            IPrinterService printerService,
            IAlertService alertService,
            ILogger<PrinterStatusCheckJob> logger)
        {
            _printerService = printerService;
            _alertService = alertService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Iniciando verificación periódica de estado de impresoras");

                // Obtener todas las impresoras
                var printers = await _printerService.GetAllPrintersAsync();

                foreach (var printer in printers)
                {
                    // Aquí obtendríamos el estado anterior de alguna caché o BD
                    // Por simplicidad, asumimos que viene de la verificación previa
                    var previousStatus = PrinterStatus.Online; // Esto debería venir de caché

                    await _alertService.ProcessPrinterAlertAsync(printer, previousStatus);
                }

                _logger.LogInformation("Verificación de estado de impresoras completada ({Count} impresoras verificadas)", printers.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando estado de impresoras");
                throw;
            }
        }
    }

    /// <summary>
    /// Job para verificación de métricas del sistema
    /// </summary>
    [DisallowConcurrentExecution]
    public class SystemMetricsCheckJob : IJob
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly IAlertService _alertService;
        private readonly ILogger<SystemMetricsCheckJob> _logger;

        public SystemMetricsCheckJob(
            IHealthCheckService healthCheckService,
            IAlertService alertService,
            ILogger<SystemMetricsCheckJob> logger)
        {
            _healthCheckService = healthCheckService;
            _alertService = alertService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Iniciando verificación de métricas del sistema");

                // Obtener métricas actuales del sistema
                var metrics = await GetCurrentSystemMetricsAsync();

                await _alertService.ProcessSystemMetricsAlertAsync(metrics);

                _logger.LogInformation("Verificación de métricas del sistema completada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando métricas del sistema");
                throw;
            }
        }

        private async Task<SystemMetrics> GetCurrentSystemMetricsAsync()
        {
            // Obtener métricas reales del sistema
            var process = System.Diagnostics.Process.GetCurrentProcess();

            return new SystemMetrics
            {
                CpuUsage = await GetCpuUsageAsync(),
                MemoryUsage = (double)process.WorkingSet64 / GetTotalMemoryBytes() * 100,
                Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
                DatabaseHealthy = await CheckDatabaseHealthAsync(),
                ScheduledReportsHealthy = await CheckScheduledReportsHealthAsync()
            };
        }

        private async Task<double> GetCpuUsageAsync()
        {
            // Esta es una implementación simplificada
            // En producción usarías PerformanceCounter o similar
            return 25.0; // Valor simulado
        }

        private static long GetTotalMemoryBytes()
        {
            return 8L * 1024 * 1024 * 1024; // 8GB simulado
        }

        private async Task<bool> CheckDatabaseHealthAsync()
        {
            try
            {
                var health = await _healthCheckService.GetBasicHealthAsync();
                return health.Status == "Healthy";
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CheckScheduledReportsHealthAsync()
        {
            try
            {
                // Verificar que el servicio de reportes programados esté funcionando
                return true; // Implementación simplificada
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Job para reentrenamiento automático del modelo de ML
    /// </summary>
    [DisallowConcurrentExecution]
    public class ModelRetrainingJob : IJob
    {
        private readonly IModelRetrainingService _retrainingService;
        private readonly ILogger<ModelRetrainingJob> _logger;

        public ModelRetrainingJob(
            IModelRetrainingService retrainingService,
            ILogger<ModelRetrainingJob> logger)
        {
            _retrainingService = retrainingService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Iniciando reentrenamiento automático del modelo ML");

                var result = await _retrainingService.RetrainModelAsync();

                if (result.ModelUpdated)
                {
                    _logger.LogInformation("Modelo ML reentrenado exitosamente. Versión: {Version}, Mejora: {Improvement:P2}",
                        result.NewModelVersion, result.ImprovementFromPrevious);
                }
                else
                {
                    _logger.LogInformation("Reentrenamiento completado sin actualización del modelo");
                }

                _logger.LogInformation("Reentrenamiento completado: {TrainingSize} datos entrenamiento, {FeedbackSize} feedback, duración: {Duration}",
                    result.TrainingDataSize, result.FeedbackDataSize, result.Duration);

                if (result.IssuesFound.Any())
                {
                    foreach (var issue in result.IssuesFound)
                    {
                        _logger.LogWarning("Problema en reentrenamiento: {Issue}", issue);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en reentrenamiento automático del modelo ML");
                throw;
            }
        }
    }

    /// <summary>
    /// Job para colección periódica de métricas de telemetría
    /// </summary>
    [DisallowConcurrentExecution]
    public class TelemetryCollectionJob : IJob
    {
        private readonly ITelemetryCollectorService _telemetryService;
        private readonly ITelemetryDataCleaner _dataCleaner;
        private readonly ILogger<TelemetryCollectionJob> _logger;

        public TelemetryCollectionJob(
            ITelemetryCollectorService telemetryService,
            ITelemetryDataCleaner dataCleaner,
            ILogger<TelemetryCollectionJob> logger)
        {
            _telemetryService = telemetryService;
            _dataCleaner = dataCleaner;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Iniciando colección periódica de métricas de telemetría");

                var collectionResult = await _telemetryService.CollectAllPrinterMetricsAsync();

                _logger.LogInformation("Métricas recolectadas: {Successful}/{Total} impresoras exitosas en {Duration}",
                    collectionResult.SuccessfulCollections, collectionResult.ActivePrinters, collectionResult.Duration);

                _telemetryService.CleanupRecentMetrics();

                _logger.LogInformation("Colección de telemetría completada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en colección periódica de métricas de telemetría");
                throw;
            }
        }
    }
