using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de métricas completas de rendimiento
    /// </summary>
    public interface IComprehensiveMetricsService
    {
        /// <summary>
        /// Registra métricas completas de una operación HTTP
        /// </summary>
        void RecordHttpOperation(string method, string endpoint, int statusCode, TimeSpan duration, string? userId = null, long? requestSize = null, long? responseSize = null);

        /// <summary>
        /// Registra métricas completas de operación de base de datos
        /// </summary>
        void RecordDatabaseOperation(string operation, string tableName, DatabaseOperationType type, TimeSpan duration, int rowsAffected, bool success, string? userId = null);

        /// <summary>
        /// Registra métricas completas de operación de IA
        /// </summary>
        void RecordAiOperation(string modelType, string operation, AiOperationResult result, TimeSpan duration, Dictionary<string, object>? metrics = null, string? userId = null);

        /// <summary>
        /// Registra métricas de sistema en tiempo real
        /// </summary>
        Task<SystemMetricsSnapshot> GetCurrentSystemMetricsAsync();

        /// <summary>
        /// Registra métricas de jobs programados
        /// </summary>
        void RecordScheduledJob(string jobType, string jobName, JobExecutionResult result, TimeSpan duration, int itemsProcessed = 0);

        /// <summary>
        /// Obtiene métricas históricas para análisis
        /// </summary>
        Task<HistoricalMetricsReport> GetHistoricalMetricsAsync(DateTime fromDate, DateTime toDate);
    }
}
