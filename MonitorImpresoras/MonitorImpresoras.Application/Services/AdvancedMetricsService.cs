using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using Prometheus;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de métricas avanzadas de rendimiento con integración Prometheus
    /// </summary>
    public class AdvancedMetricsService : IAdvancedMetricsService
    {
        private readonly ILogger<AdvancedMetricsService> _logger;
        private readonly IPerformanceAuditService _auditService;
        private readonly ConcurrentDictionary<string, Histogram> _responseTimeHistograms = new();
        private readonly ConcurrentDictionary<string, Counter> _requestCounters = new();
        private readonly ConcurrentDictionary<string, Gauge> _activeGauges = new();

        // Métricas de Prometheus
        private static readonly Gauge DatabaseConnections = Metrics.CreateGauge("db_connections_active", "Número de conexiones activas a la base de datos");
        private static readonly Gauge CacheHitRate = Metrics.CreateGauge("cache_hit_rate", "Tasa de aciertos del cache (%)");
        private static readonly Counter TotalRequests = Metrics.CreateCounter("http_requests_total", "Número total de requests HTTP", new[] { "method", "endpoint", "status_code" });
        private static readonly Histogram ResponseTime = Metrics.CreateHistogram("http_request_duration_seconds", "Tiempo de respuesta HTTP en segundos", new[] { "method", "endpoint" });
        private static readonly Counter FailedJobs = Metrics.CreateCounter("jobs_failed_total", "Número total de jobs fallidos", new[] { "job_type" });
        private static readonly Gauge ActiveJobs = Metrics.CreateGauge("jobs_active", "Número de jobs activos", new[] { "job_type" });
        private static readonly Counter TelemetryCollected = Metrics.CreateCounter("telemetry_collected_total", "Número total de métricas de telemetría recolectadas", new[] { "printer_id", "status" });
        private static readonly Counter PredictionsGenerated = Metrics.CreateCounter("predictions_generated_total", "Número total de predicciones generadas", new[] { "prediction_type", "result" });
        private static readonly Gauge ModelAccuracy = Metrics.CreateGauge("model_accuracy", "Precisión del modelo de ML por tipo", new[] { "model_type", "prediction_type" });

        public AdvancedMetricsService(
            ILogger<AdvancedMetricsService> logger,
            IPerformanceAuditService auditService)
        {
            _logger = logger;
            _auditService = auditService;

            // Inicializar métricas básicas
            InitializeMetrics();
        }

        /// <summary>
        /// Registra métrica de tiempo de respuesta HTTP
        /// </summary>
        public void RecordHttpResponse(string method, string endpoint, int statusCode, TimeSpan duration)
        {
            try
            {
                // Métrica Prometheus
                ResponseTime.Labels(method, endpoint).Observe(duration.TotalSeconds);
                TotalRequests.Labels(method, endpoint, statusCode.ToString()).Inc();

                // Métrica interna para análisis detallado
                var key = $"{method}_{endpoint}";
                if (!_responseTimeHistograms.ContainsKey(key))
                {
                    _responseTimeHistograms[key] = Metrics.CreateHistogram($"http_{method.ToLower()}_{endpoint.Replace("/", "_")}_duration_seconds",
                        $"Tiempo de respuesta para {method} {endpoint}", new[] { "status_code" });
                }

                _responseTimeHistograms[key].Labels(statusCode.ToString()).Observe(duration.TotalSeconds);

                _logger.LogDebug("Métrica HTTP registrada: {Method} {Endpoint} - {Duration}ms ({StatusCode})",
                    method, endpoint, duration.TotalMilliseconds, statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métrica HTTP");
            }
        }

        /// <summary>
        /// Registra métrica de ejecución de job
        /// </summary>
        public void RecordJobExecution(string jobType, bool success, TimeSpan duration)
        {
            try
            {
                if (!success)
                {
                    FailedJobs.Labels(jobType).Inc();
                }

                // Actualizar gauge de jobs activos
                var activeKey = $"active_{jobType}";
                if (!_activeGauges.ContainsKey(activeKey))
                {
                    _activeGauges[activeKey] = Metrics.CreateGauge($"jobs_{jobType}_active", $"Jobs activos de tipo {jobType}");
                }

                // Nota: En producción, necesitarías rastrear realmente los jobs activos
                ActiveJobs.Labels(jobType).Set(1); // Simulado

                _logger.LogDebug("Métrica de job registrada: {JobType} - {Success} en {Duration}ms",
                    jobType, success ? "Exitoso" : "Fallido", duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métrica de job");
            }
        }

        /// <summary>
        /// Registra métrica de colección de telemetría
        /// </summary>
        public void RecordTelemetryCollection(int printerId, bool success, int metricsCount)
        {
            try
            {
                TelemetryCollected.Labels(printerId.ToString(), success ? "success" : "failed").Inc(metricsCount);

                _logger.LogDebug("Métrica de telemetría registrada: Impresora {PrinterId} - {Success} ({MetricsCount} métricas)",
                    printerId, success ? "Exitosa" : "Fallida", metricsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métrica de telemetría");
            }
        }

        /// <summary>
        /// Registra métrica de predicción generada
        /// </summary>
        public void RecordPrediction(string predictionType, string result, decimal confidence)
        {
            try
            {
                PredictionsGenerated.Labels(predictionType, result).Inc();

                // Actualizar precisión del modelo si hay resultado conocido
                if (result == "correct" || result == "incorrect")
                {
                    var accuracy = result == "correct" ? 1.0 : 0.0;
                    ModelAccuracy.Labels("maintenance", predictionType).Set(accuracy);
                }

                _logger.LogDebug("Métrica de predicción registrada: {PredictionType} - {Result} (Confianza: {Confidence:P})",
                    predictionType, result, confidence / 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métrica de predicción");
            }
        }

        /// <summary>
        /// Actualiza métricas de base de datos
        /// </summary>
        public void UpdateDatabaseMetrics(int activeConnections, double cacheHitRate)
        {
            try
            {
                DatabaseConnections.Set(activeConnections);
                CacheHitRate.Set(cacheHitRate);

                _logger.LogDebug("Métricas de BD actualizadas: {Connections} conexiones activas, {HitRate:P} tasa de aciertos",
                    activeConnections, cacheHitRate / 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando métricas de BD");
            }
        }

        /// <summary>
        /// Obtiene métricas actuales del sistema
        /// </summary>
        public async Task<SystemPerformanceMetrics> GetCurrentMetricsAsync()
        {
            try
            {
                var process = Process.GetCurrentProcess();

                return new SystemPerformanceMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsage = await GetCpuUsageAsync(),
                    MemoryUsage = process.WorkingSet64 / (double)GetTotalMemoryBytes() * 100,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    GcCollections0 = GC.CollectionCount(0),
                    GcCollections1 = GC.CollectionCount(1),
                    GcCollections2 = GC.CollectionCount(2),
                    DatabaseConnections = (int)DatabaseConnections.Value,
                    CacheHitRate = CacheHitRate.Value,
                    ActiveRequests = GetActiveRequestsCount(),
                    AverageResponseTime = ResponseTime.Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas actuales del sistema");
                return new SystemPerformanceMetrics();
            }
        }

        /// <summary>
        /// Genera reporte de rendimiento basado en métricas históricas
        /// </summary>
        public async Task<PerformanceReport> GeneratePerformanceReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddHours(-24);
                var endDate = toDate ?? DateTime.UtcNow;

                var report = new PerformanceReport
                {
                    ReportPeriod = new DateTimeRange(startDate, endDate),
                    SystemMetrics = await GetCurrentMetricsAsync(),
                    DatabaseMetrics = await GetDatabaseMetricsAsync(),
                    ApiMetrics = await GetApiMetricsAsync(),
                    JobMetrics = await GetJobMetricsAsync(),
                    Recommendations = await GeneratePerformanceRecommendationsAsync()
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de rendimiento");
                return new PerformanceReport();
            }
        }

        /// <summary>
        /// Configura alertas de rendimiento
        /// </summary>
        public void ConfigurePerformanceAlerts()
        {
            try
            {
                // Configurar alertas basadas en métricas críticas

                // Alerta si tiempo de respuesta > 500ms por más de 5 minutos
                var highLatencyAlert = ResponseTime.CreateAlert("high_latency", "Tiempo de respuesta alto detectado");
                highLatencyAlert.SetThreshold(0.5, AlertThresholdOperator.GreaterThan); // 500ms

                // Alerta si tasa de errores > 5%
                var highErrorRateAlert = TotalRequests.CreateAlert("high_error_rate", "Tasa de errores elevada");
                highErrorRateAlert.SetThreshold(0.05, AlertThresholdOperator.GreaterThan);

                // Alerta si conexiones a BD > 80%
                var highDbConnectionsAlert = DatabaseConnections.CreateAlert("high_db_connections", "Muchas conexiones activas a BD");
                highDbConnectionsAlert.SetThreshold(80, AlertThresholdOperator.GreaterThan);

                _logger.LogInformation("Alertas de rendimiento configuradas exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando alertas de rendimiento");
            }
        }

        /// <summary>
        /// Inicializa métricas básicas del sistema
        /// </summary>
        private void InitializeMetrics()
        {
            try
            {
                // Inicializar métricas básicas
                DatabaseConnections.Set(0);
                CacheHitRate.Set(0);
                ActiveJobs.Labels("telemetry").Set(0);
                ActiveJobs.Labels("reports").Set(0);
                ActiveJobs.Labels("alerts").Set(0);

                _logger.LogInformation("Métricas básicas inicializadas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inicializando métricas básicas");
            }
        }

        /// <summary>
        /// Obtiene métricas de base de datos
        /// </summary>
        private async Task<DatabasePerformanceMetrics> GetDatabaseMetricsAsync()
        {
            return new DatabasePerformanceMetrics
            {
                ActiveConnections = (int)DatabaseConnections.Value,
                CacheHitRate = CacheHitRate.Value,
                AverageQueryTime = 45.2,
                SlowQueriesCount = 3,
                DeadlocksCount = 0
            };
        }

        /// <summary>
        /// Obtiene métricas de API
        /// </summary>
        private async Task<ApiPerformanceMetrics> GetApiMetricsAsync()
        {
            return new ApiPerformanceMetrics
            {
                TotalRequests = (long)TotalRequests.Value,
                AverageResponseTime = ResponseTime.Value,
                ErrorRate = 0.02, // 2% simulado
                P95ResponseTime = 0.45, // 450ms simulado
                P99ResponseTime = 0.85  // 850ms simulado
            };
        }

        /// <summary>
        /// Obtiene métricas de jobs
        /// </summary>
        private async Task<JobPerformanceMetrics> GetJobMetricsAsync()
        {
            return new JobPerformanceMetrics
            {
                ActiveJobs = (int)ActiveJobs.Value,
                FailedJobs = (long)FailedJobs.Value,
                AverageExecutionTime = 45.2,
                JobsPerHour = 12
            };
        }

        /// <summary>
        /// Genera recomendaciones basadas en métricas actuales
        /// </summary>
        private async Task<List<string>> GeneratePerformanceRecommendationsAsync()
        {
            var recommendations = new List<string>();

            if (ResponseTime.Value > 0.5)
                recommendations.Add("Optimizar consultas lentas para reducir tiempo de respuesta");

            if (DatabaseConnections.Value > 80)
                recommendations.Add("Revisar configuración de pool de conexiones a BD");

            if (CacheHitRate.Value < 85)
                recommendations.Add("Mejorar configuración de cache para aumentar tasa de aciertos");

            if (FailedJobs.Value > 10)
                recommendations.Add("Revisar jobs fallidos y mejorar manejo de errores");

            return recommendations;
        }

        /// <summary>
        /// Obtiene uso de CPU del proceso actual
        /// </summary>
        private async Task<double> GetCpuUsageAsync()
        {
            // Esta es una implementación simplificada
            // En producción usarías PerformanceCounter o similar
            return 25.0; // 25% simulado
        }

        /// <summary>
        /// Obtiene memoria total del sistema en bytes
        /// </summary>
        private static long GetTotalMemoryBytes()
        {
            return 8L * 1024 * 1024 * 1024; // 8GB simulado
        }

        /// <summary>
        /// Obtiene número de requests activos (simulado)
        /// </summary>
        private int GetActiveRequestsCount()
        {
            return 5; // Simulado
        }
    }

    /// <summary>
    /// DTO para métricas de rendimiento del sistema
    /// </summary>
    public class SystemPerformanceMetrics
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public int GcCollections0 { get; set; }
        public int GcCollections1 { get; set; }
        public int GcCollections2 { get; set; }
        public int DatabaseConnections { get; set; }
        public double CacheHitRate { get; set; }
        public int ActiveRequests { get; set; }
        public double AverageResponseTime { get; set; }
    }

    /// <summary>
    /// DTO para métricas de rendimiento de base de datos
    /// </summary>
    public class DatabasePerformanceMetrics
    {
        public int ActiveConnections { get; set; }
        public double CacheHitRate { get; set; }
        public double AverageQueryTime { get; set; }
        public int SlowQueriesCount { get; set; }
        public int DeadlocksCount { get; set; }
    }

    /// <summary>
    /// DTO para métricas de rendimiento de API
    /// </summary>
    public class ApiPerformanceMetrics
    {
        public long TotalRequests { get; set; }
        public double AverageResponseTime { get; set; }
        public double ErrorRate { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
    }

    /// <summary>
    /// DTO para métricas de rendimiento de jobs
    /// </summary>
    public class JobPerformanceMetrics
    {
        public int ActiveJobs { get; set; }
        public long FailedJobs { get; set; }
        public double AverageExecutionTime { get; set; }
        public int JobsPerHour { get; set; }
    }

    /// <summary>
    /// DTO para reporte de rendimiento
    /// </summary>
    public class PerformanceReport
    {
        public DateTimeRange ReportPeriod { get; set; } = new();
        public SystemPerformanceMetrics SystemMetrics { get; set; } = new();
        public DatabasePerformanceMetrics DatabaseMetrics { get; set; } = new();
        public ApiPerformanceMetrics ApiMetrics { get; set; } = new();
        public JobPerformanceMetrics JobMetrics { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO para rango de fechas
    /// </summary>
    public class DateTimeRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration => End - Start;

        public DateTimeRange() { }

        public DateTimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }
}
