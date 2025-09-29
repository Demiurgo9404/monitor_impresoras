using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para almacenar métricas de telemetría de impresoras
    /// </summary>
    [Table("PrinterTelemetry")]
    public class PrinterTelemetry
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// ID de la impresora a la que pertenece esta métrica
        /// </summary>
        [Required]
        public int PrinterId { get; set; }

        /// <summary>
        /// ID del tenant al que pertenece esta métrica
        /// </summary>
        [Required]
        public int TenantId { get; set; } = 1;

        /// <summary>
        /// Marca de tiempo UTC cuando se capturó la métrica
        /// </summary>
        [Required]
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Número de páginas impresas desde el último registro
        /// </summary>
        public int? PagesPrinted { get; set; }

        /// <summary>
        /// Nivel de tóner actual (0-100%)
        /// </summary>
        public int? TonerLevel { get; set; }

        /// <summary>
        /// Nivel de papel actual (0-100%)
        /// </summary>
        public int? PaperLevel { get; set; }

        /// <summary>
        /// Número de errores detectados desde el último registro
        /// </summary>
        public int? ErrorsCount { get; set; }

        /// <summary>
        /// Estado actual de la impresora
        /// </summary>
        [Required, MaxLength(20)]
        public string Status { get; set; } = default!;

        /// <summary>
        /// Temperatura del dispositivo (si aplica)
        /// </summary>
        public decimal? Temperature { get; set; }

        /// <summary>
        /// Uso de CPU del dispositivo (si aplica, 0-100%)
        /// </summary>
        public decimal? CpuUsage { get; set; }

        /// <summary>
        /// Uso de memoria del dispositivo (si aplica, 0-100%)
        /// </summary>
        public decimal? MemoryUsage { get; set; }

        /// <summary>
        /// Número de trabajos de impresión en cola
        /// </summary>
        public int? JobsInQueue { get; set; }

        /// <summary>
        /// Tiempo promedio de respuesta en milisegundos
        /// </summary>
        public long? AverageResponseTimeMs { get; set; }

        /// <summary>
        /// IP de la impresora en el momento de la captura
        /// </summary>
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// MAC address de la impresora (si aplica)
        /// </summary>
        [MaxLength(17)]
        public string? MacAddress { get; set; }

        /// <summary>
        /// Información adicional específica del modelo
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? AdditionalMetrics { get; set; }

        /// <summary>
        /// Método usado para capturar la métrica
        /// </summary>
        [Required, MaxLength(50)]
        public string CollectionMethod { get; set; } = "SNMP";

        /// <summary>
        /// Si la métrica fue capturada exitosamente
        /// </summary>
        public bool CollectionSuccessful { get; set; } = true;

        /// <summary>
        /// Mensaje de error si la captura falló
        /// </summary>
        [MaxLength(1000)]
        public string? CollectionErrorMessage { get; set; }

        /// <summary>
        /// Tiempo que tomó capturar la métrica en milisegundos
        /// </summary>
        public int? CollectionTimeMs { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navegación a la impresora
        /// </summary>
        [ForeignKey("PrinterId")]
        public virtual Printer? Printer { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public PrinterTelemetry() { }

        /// <summary>
        /// Constructor con parámetros principales
        /// </summary>
        public PrinterTelemetry(int printerId, DateTime timestamp, string status)
        {
            PrinterId = printerId;
            TimestampUtc = timestamp;
            Status = status;
        }

        /// <summary>
        /// Calcula el tiempo transcurrido desde la captura
        /// </summary>
        public TimeSpan TimeSinceCapture => DateTime.UtcNow - TimestampUtc;

        /// <summary>
        /// Verifica si la métrica es reciente (menos de 5 minutos)
        /// </summary>
        public bool IsRecent => TimeSinceCapture.TotalMinutes < 5;

        /// <summary>
        /// Obtiene el estado como enum si es válido
        /// </summary>
        public PrinterStatus? GetStatusAsEnum()
        {
            if (Enum.TryParse<PrinterStatus>(Status, true, out var status))
                return status;
            return null;
        }
    }

    /// <summary>
    /// Entidad para datos de telemetría limpios y normalizados
    /// </summary>
    [Table("PrinterTelemetryClean")]
    public class PrinterTelemetryClean
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// ID de la impresora
        /// </summary>
        [Required]
        public int PrinterId { get; set; }

        /// <summary>
        /// Timestamp normalizado (cada 5 minutos)
        /// </summary>
        [Required]
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Promedio de páginas impresas en el período
        /// </summary>
        public decimal? AvgPagesPrinted { get; set; }

        /// <summary>
        /// Promedio del nivel de tóner en el período
        /// </summary>
        public decimal? AvgTonerLevel { get; set; }

        /// <summary>
        /// Promedio del nivel de papel en el período
        /// </summary>
        public decimal? AvgPaperLevel { get; set; }

        /// <summary>
        /// Número total de errores en el período
        /// </summary>
        public int? TotalErrors { get; set; }

        /// <summary>
        /// Estado más común en el período
        /// </summary>
        [Required, MaxLength(20)]
        public string DominantStatus { get; set; } = default!;

        /// <summary>
        /// Promedio de temperatura en el período
        /// </summary>
        public decimal? AvgTemperature { get; set; }

        /// <summary>
        /// Promedio de uso de CPU en el período
        /// </summary>
        public decimal? AvgCpuUsage { get; set; }

        /// <summary>
        /// Promedio de uso de memoria en el período
        /// </summary>
        public decimal? AvgMemoryUsage { get; set; }

        /// <summary>
        /// Número promedio de trabajos en cola
        /// </summary>
        public decimal? AvgJobsInQueue { get; set; }

        /// <summary>
        /// Promedio del tiempo de respuesta en el período
        /// </summary>
        public long? AvgResponseTimeMs { get; set; }

        /// <summary>
        /// Número de muestras usadas para calcular los promedios
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// Calidad de los datos (0-100, basado en completitud y consistencia)
        /// </summary>
        public decimal DataQualityScore { get; set; }

        /// <summary>
        /// Fecha de creación del registro limpio
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navegación a la impresora
        /// </summary>
        [ForeignKey("PrinterId")]
        public virtual Printer? Printer { get; set; }
    }

    /// <summary>
    /// Entidad para predicciones de mantenimiento
    /// </summary>
    [Table("MaintenancePrediction")]
    public class MaintenancePrediction
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// ID de la impresora para la que se hace la predicción
        /// </summary>
        [Required]
        public int PrinterId { get; set; }

        /// <summary>
        /// Fecha en que se generó la predicción
        /// </summary>
        [Required]
        public DateTime PredictedAt { get; set; }

        /// <summary>
        /// Tipo de predicción realizada
        /// </summary>
        [Required, MaxLength(50)]
        public string PredictionType { get; set; } = default!;

        /// <summary>
        /// Probabilidad de que ocurra el evento (0-1)
        /// </summary>
        [Required]
        public decimal Probability { get; set; }

        /// <summary>
        /// Fecha estimada del evento
        /// </summary>
        public DateTime? EstimatedDate { get; set; }

        /// <summary>
        /// Días hasta el evento estimado
        /// </summary>
        public int? DaysUntilEvent { get; set; }

        /// <summary>
        /// Confianza del modelo en la predicción (0-1)
        /// </summary>
        public decimal Confidence { get; set; }

        /// <summary>
        /// Modelo de ML usado para la predicción
        /// </summary>
        [MaxLength(100)]
        public string ModelVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Datos de entrada usados para la predicción
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? InputData { get; set; }

        /// <summary>
        /// Características del modelo que contribuyeron más a la predicción
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? FeatureImportance { get; set; }

        /// <summary>
        /// Acción recomendada basada en la predicción
        /// </summary>
        [MaxLength(200)]
        public string? RecommendedAction { get; set; }

        /// <summary>
        /// Si la predicción fue revisada por un humano
        /// </summary>
        public bool HumanReviewed { get; set; }

        /// <summary>
        /// Usuario que revisó la predicción
        /// </summary>
        [MaxLength(450)]
        public string? ReviewedBy { get; set; }

        /// <summary>
        /// Comentarios de la revisión humana
        /// </summary>
        [MaxLength(1000)]
        public string? ReviewComments { get; set; }

        /// <summary>
        /// Fecha de revisión humana
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Si la predicción resultó ser correcta
        /// </summary>
        public bool? PredictionAccuracy { get; set; }

        /// <summary>
        /// Fecha cuando se confirmó la precisión de la predicción
        /// </summary>
        public DateTime? AccuracyConfirmedAt { get; set; }

        /// <summary>
        /// Comentarios sobre la precisión de la predicción
        /// </summary>
        [MaxLength(1000)]
        public string? AccuracyComments { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navegación a la impresora
        /// </summary>
        [ForeignKey("PrinterId")]
        public virtual Printer? Printer { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public MaintenancePrediction() { }

        /// <summary>
        /// Constructor con parámetros principales
        /// </summary>
        public MaintenancePrediction(int printerId, string predictionType, decimal probability)
        {
            PrinterId = printerId;
            PredictionType = predictionType;
            Probability = probability;
            PredictedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Obtiene la severidad basada en la probabilidad
        /// </summary>
        public PredictionSeverity GetSeverity()
        {
            return Probability switch
            {
                >= 0.8m => PredictionSeverity.Critical,
                >= 0.6m => PredictionSeverity.High,
                >= 0.4m => PredictionSeverity.Medium,
                _ => PredictionSeverity.Low
            };
        }

        /// <summary>
        /// Verifica si la predicción es reciente (menos de 24 horas)
        /// </summary>
        public bool IsRecent => DateTime.UtcNow - PredictedAt < TimeSpan.FromDays(1);

        /// <summary>
        /// Verifica si requiere atención inmediata
        /// </summary>
        public bool RequiresImmediateAttention => Probability >= 0.7m && DaysUntilEvent <= 3;
    }

    /// <summary>
    /// Severidades de predicción
    /// </summary>
    public enum PredictionSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Tipos de predicción de mantenimiento
    /// </summary>
    public enum PredictionType
    {
        TonerDepletion,
        PaperDepletion,
        NetworkFailure,
        HardwareFailure,
        ServiceDisruption,
        MaintenanceRequired
    }
}
