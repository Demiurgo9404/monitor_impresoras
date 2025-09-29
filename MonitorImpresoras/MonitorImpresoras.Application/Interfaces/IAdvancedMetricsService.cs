using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de métricas avanzadas de rendimiento
    /// </summary>
    public interface IAdvancedMetricsService
    {
        /// <summary>
        /// Registra métrica de tiempo de respuesta HTTP
        /// </summary>
        void RecordHttpResponse(string method, string endpoint, int statusCode, TimeSpan duration);

        /// <summary>
        /// Registra métrica de ejecución de job
        /// </summary>
        void RecordJobExecution(string jobType, bool success, TimeSpan duration);

        /// <summary>
        /// Registra métrica de colección de telemetría
        /// </summary>
        void RecordTelemetryCollection(int printerId, bool success, int metricsCount);

        /// <summary>
        /// Registra métrica de predicción generada
        /// </summary>
        void RecordPrediction(string predictionType, string result, decimal confidence);

        /// <summary>
        /// Actualiza métricas de base de datos
        /// </summary>
        void UpdateDatabaseMetrics(int activeConnections, double cacheHitRate);

        /// <summary>
        /// Obtiene métricas actuales del sistema
        /// </summary>
        Task<SystemPerformanceMetrics> GetCurrentMetricsAsync();

        /// <summary>
        /// Genera reporte de rendimiento basado en métricas históricas
        /// </summary>
        Task<PerformanceReport> GeneratePerformanceReportAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Configura alertas de rendimiento
        /// </summary>
        void ConfigurePerformanceAlerts();
    }
}
