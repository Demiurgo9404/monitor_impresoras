using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de colección de métricas de telemetría
    /// </summary>
    public interface ITelemetryCollectorService
    {
        /// <summary>
        /// Captura métricas de todas las impresoras activas
        /// </summary>
        Task<TelemetryCollectionResult> CollectAllPrinterMetricsAsync();

        /// <summary>
        /// Captura métricas de una impresora específica
        /// </summary>
        Task<PrinterTelemetry?> CollectPrinterMetricsAsync(Printer printer);

        /// <summary>
        /// Obtiene métricas recientes para análisis predictivo
        /// </summary>
        Task<IEnumerable<TelemetryDataPoint>> GetRecentMetricsAsync(int printerId, TimeSpan timeWindow);

        /// <summary>
        /// Obtiene estadísticas de colección de métricas
        /// </summary>
        Task<TelemetryCollectionStatistics> GetCollectionStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Limpia métricas antiguas de memoria
        /// </summary>
        void CleanupRecentMetrics();
    }

    /// <summary>
    /// DTO para métricas de impresora capturadas
    /// </summary>
    public class PrinterMetrics
    {
        public int? TonerLevel { get; set; }
        public int? PaperLevel { get; set; }
        public int? PagesPrinted { get; set; }
        public int? ErrorsCount { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? CpuUsage { get; set; }
        public decimal? MemoryUsage { get; set; }
        public int? JobsInQueue { get; set; }
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// DTO para punto de datos de telemetría simplificado
    /// </summary>
    public class TelemetryDataPoint
    {
        public int PrinterId { get; set; }
        public DateTime Timestamp { get; set; }
        public int? TonerLevel { get; set; }
        public int? PaperLevel { get; set; }
        public int? ErrorsCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resultado de colección de métricas
    /// </summary>
    public class TelemetryCollectionResult
    {
        public DateTime CollectionStartTime { get; set; }
        public DateTime CollectionEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int TotalPrinters { get; set; }
        public int ActivePrinters { get; set; }
        public int SuccessfulCollections { get; set; }
        public int FailedCollections { get; set; }
        public int TelemetryPointsCollected { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de colección de métricas
    /// </summary>
    public class TelemetryCollectionStatistics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalCollections { get; set; }
        public int SuccessfulCollections { get; set; }
        public int FailedCollections { get; set; }
        public double AverageCollectionTimeMs { get; set; }
        public string MostUsedMethod { get; set; } = string.Empty;
        public Dictionary<string, int> MethodsUsed { get; set; } = new();
    }
}
