using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de limpieza y normalización de datos de telemetría
    /// </summary>
    public interface ITelemetryDataCleaner
    {
        /// <summary>
        /// Limpia y normaliza datos de telemetría crudos
        /// </summary>
        Task<TelemetryCleaningResult> CleanAndNormalizeTelemetryAsync(IEnumerable<PrinterTelemetry> rawData);

        /// <summary>
        /// Obtiene estadísticas de calidad de datos
        /// </summary>
        Task<DataQualityStatistics> GetDataQualityStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

    /// <summary>
    /// DTO para resultado de limpieza de datos
    /// </summary>
    public class TelemetryCleaningResult
    {
        public DateTime ProcessingStartTime { get; set; }
        public DateTime ProcessingEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int TotalRawRecords { get; set; }
        public int TotalCleanRecords { get; set; }
        public int OutliersRemoved { get; set; }
        public int InvalidRecords { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de calidad de datos
    /// </summary>
    public class DataQualityStatistics
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal AverageDataQuality { get; set; }
        public int RecordsWithHighQuality { get; set; }
        public int RecordsWithMediumQuality { get; set; }
        public int RecordsWithLowQuality { get; set; }
        public List<string> MostCommonIssues { get; set; } = new();
    }
}
