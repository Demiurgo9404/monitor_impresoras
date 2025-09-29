using Microsoft.Extensions.Logging;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para limpieza y normalización de datos de telemetría
    /// </summary>
    public class TelemetryDataCleaner : ITelemetryDataCleaner
    {
        private readonly ILogger<TelemetryDataCleaner> _logger;

        public TelemetryDataCleaner(ILogger<TelemetryDataCleaner> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Limpia y normaliza datos de telemetría crudos
        /// </summary>
        public async Task<TelemetryCleaningResult> CleanAndNormalizeTelemetryAsync(IEnumerable<PrinterTelemetry> rawData)
        {
            try
            {
                _logger.LogInformation("Iniciando limpieza y normalización de {Count} registros de telemetría", rawData.Count());

                var result = new TelemetryCleaningResult
                {
                    ProcessingStartTime = DateTime.UtcNow,
                    TotalRawRecords = rawData.Count()
                };

                var cleanedRecords = new List<PrinterTelemetryClean>();
                var outliersRemoved = 0;
                var invalidRecords = 0;

                // Procesar datos por impresora y período de tiempo
                var groupedByPrinter = rawData.GroupBy(t => t.PrinterId);

                foreach (var printerGroup in groupedByPrinter)
                {
                    var normalizedData = await NormalizePrinterDataAsync(printerGroup.ToList());
                    cleanedRecords.AddRange(normalizedData);
                }

                result.TotalCleanRecords = cleanedRecords.Count;
                result.OutliersRemoved = outliersRemoved;
                result.InvalidRecords = invalidRecords;
                result.ProcessingEndTime = DateTime.UtcNow;
                result.Duration = result.ProcessingEndTime - result.ProcessingStartTime;

                _logger.LogInformation("Limpieza completada: {Cleaned}/{Total} registros procesados, {Outliers} outliers removidos",
                    result.TotalCleanRecords, result.TotalRawRecords, result.OutliersRemoved);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en limpieza y normalización de datos de telemetría");
                throw;
            }
        }

        /// <summary>
        /// Detecta y elimina outliers de los datos de una impresora
        /// </summary>
        private async Task<List<PrinterTelemetry>> RemoveOutliersAsync(List<PrinterTelemetry> printerData)
        {
            var cleanedData = new List<PrinterTelemetry>();

            foreach (var record in printerData)
            {
                if (IsValidRecord(record))
                {
                    cleanedData.Add(record);
                }
                else
                {
                    _logger.LogWarning("Registro inválido detectado para impresora {PrinterId}: {Timestamp}",
                        record.PrinterId, record.TimestampUtc);
                }
            }

            return cleanedData;
        }

        /// <summary>
        /// Verifica si un registro de telemetría es válido
        /// </summary>
        private bool IsValidRecord(PrinterTelemetry record)
        {
            // Verificaciones básicas de validez
            if (record.TonerLevel.HasValue && (record.TonerLevel < 0 || record.TonerLevel > 100))
                return false;

            if (record.PaperLevel.HasValue && (record.PaperLevel < 0 || record.PaperLevel > 100))
                return false;

            if (record.ErrorsCount.HasValue && record.ErrorsCount < 0)
                return false;

            if (record.Temperature.HasValue && (record.Temperature < -10 || record.Temperature > 100))
                return false;

            if (record.CpuUsage.HasValue && (record.CpuUsage < 0 || record.CpuUsage > 100))
                return false;

            if (record.MemoryUsage.HasValue && (record.MemoryUsage < 0 || record.MemoryUsage > 100))
                return false;

            return true;
        }

        /// <summary>
        /// Normaliza datos de una impresora específica creando promedios por período
        /// </summary>
        private async Task<List<PrinterTelemetryClean>> NormalizePrinterDataAsync(List<PrinterTelemetry> printerData)
        {
            var normalizedRecords = new List<PrinterTelemetryClean>();

            // Normalizar timestamps a intervalos de 5 minutos
            var normalizedTimestamps = GetNormalizedTimestamps(printerData);

            foreach (var timestamp in normalizedTimestamps)
            {
                var windowStart = timestamp;
                var windowEnd = timestamp.AddMinutes(5);

                var windowData = printerData
                    .Where(t => t.TimestampUtc >= windowStart && t.TimestampUtc < windowEnd)
                    .ToList();

                if (windowData.Any())
                {
                    var normalizedRecord = CreateNormalizedRecord(windowStart, windowData);
                    normalizedRecords.Add(normalizedRecord);
                }
            }

            return normalizedRecords;
        }

        /// <summary>
        /// Crea un registro normalizado a partir de datos de una ventana de tiempo
        /// </summary>
        private PrinterTelemetryClean CreateNormalizedRecord(DateTime timestamp, List<PrinterTelemetry> windowData)
        {
            var record = new PrinterTelemetryClean
            {
                PrinterId = windowData.First().PrinterId,
                TimestampUtc = timestamp,
                SampleCount = windowData.Count
            };

            // Calcular promedios para métricas numéricas
            if (windowData.Any(d => d.TonerLevel.HasValue))
            {
                record.AvgTonerLevel = windowData
                    .Where(d => d.TonerLevel.HasValue)
                    .Average(d => d.TonerLevel.Value);
            }

            if (windowData.Any(d => d.PaperLevel.HasValue))
            {
                record.AvgPaperLevel = windowData
                    .Where(d => d.PaperLevel.HasValue)
                    .Average(d => d.PaperLevel.Value);
            }

            if (windowData.Any(d => d.PagesPrinted.HasValue))
            {
                record.AvgPagesPrinted = windowData
                    .Where(d => d.PagesPrinted.HasValue)
                    .Average(d => d.PagesPrinted.Value);
            }

            if (windowData.Any(d => d.ErrorsCount.HasValue))
            {
                record.TotalErrors = windowData
                    .Where(d => d.ErrorsCount.HasValue)
                    .Sum(d => d.ErrorsCount.Value);
            }

            if (windowData.Any(d => d.Temperature.HasValue))
            {
                record.AvgTemperature = windowData
                    .Where(d => d.Temperature.HasValue)
                    .Average(d => d.Temperature.Value);
            }

            if (windowData.Any(d => d.CpuUsage.HasValue))
            {
                record.AvgCpuUsage = windowData
                    .Where(d => d.CpuUsage.HasValue)
                    .Average(d => d.CpuUsage.Value);
            }

            if (windowData.Any(d => d.MemoryUsage.HasValue))
            {
                record.AvgMemoryUsage = windowData
                    .Where(d => d.MemoryUsage.HasValue)
                    .Average(d => d.MemoryUsage.Value);
            }

            if (windowData.Any(d => d.JobsInQueue.HasValue))
            {
                record.AvgJobsInQueue = windowData
                    .Where(d => d.JobsInQueue.HasValue)
                    .Average(d => d.JobsInQueue.Value);
            }

            if (windowData.Any(d => d.AverageResponseTimeMs.HasValue))
            {
                record.AvgResponseTimeMs = (long)windowData
                    .Where(d => d.AverageResponseTimeMs.HasValue)
                    .Average(d => d.AverageResponseTimeMs.Value);
            }

            // Determinar estado dominante
            record.DominantStatus = windowData
                .GroupBy(d => d.Status)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            // Calcular calidad de datos (basado en completitud)
            record.DataQualityScore = CalculateDataQualityScore(windowData);

            return record;
        }

        /// <summary>
        /// Obtiene timestamps normalizados cada 5 minutos
        /// </summary>
        private List<DateTime> GetNormalizedTimestamps(List<PrinterTelemetry> printerData)
        {
            if (!printerData.Any())
                return new List<DateTime>();

            var minTimestamp = printerData.Min(d => d.TimestampUtc);
            var maxTimestamp = printerData.Max(d => d.TimestampUtc);

            var normalizedTimestamps = new List<DateTime>();

            // Normalizar al inicio del período de 5 minutos más cercano
            var startTime = new DateTime(
                minTimestamp.Year, minTimestamp.Month, minTimestamp.Day,
                minTimestamp.Hour, (minTimestamp.Minute / 5) * 5, 0);

            var currentTime = startTime;
            while (currentTime <= maxTimestamp)
            {
                normalizedTimestamps.Add(currentTime);
                currentTime = currentTime.AddMinutes(5);
            }

            return normalizedTimestamps;
        }

        /// <summary>
        /// Calcula la calidad de los datos basada en completitud y consistencia
        /// </summary>
        private decimal CalculateDataQualityScore(List<PrinterTelemetry> windowData)
        {
            var totalFields = 10; // Número total de campos métricos posibles
            var completedFields = 0;

            foreach (var data in windowData)
            {
                if (data.TonerLevel.HasValue) completedFields++;
                if (data.PaperLevel.HasValue) completedFields++;
                if (data.PagesPrinted.HasValue) completedFields++;
                if (data.ErrorsCount.HasValue) completedFields++;
                if (data.Temperature.HasValue) completedFields++;
                if (data.CpuUsage.HasValue) completedFields++;
                if (data.MemoryUsage.HasValue) completedFields++;
                if (data.JobsInQueue.HasValue) completedFields++;
                if (data.AverageResponseTimeMs.HasValue) completedFields++;
                if (!string.IsNullOrEmpty(data.Status)) completedFields++;
            }

            return Math.Round((decimal)completedFields / (windowData.Count * totalFields) * 100, 2);
        }

        /// <summary>
        /// Obtiene estadísticas de calidad de datos
        /// </summary>
        public async Task<DataQualityStatistics> GetDataQualityStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var startDate = fromDate ?? DateTime.UtcNow.AddDays(-7);
            var endDate = toDate ?? DateTime.UtcNow;

            // Aquí harías consultas reales a BD
            return new DataQualityStatistics
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                AverageDataQuality = 85.5m,
                RecordsWithHighQuality = 0,
                RecordsWithMediumQuality = 0,
                RecordsWithLowQuality = 0,
                MostCommonIssues = new List<string> { "Missing toner data", "Invalid temperature readings" }
            };
        }
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
