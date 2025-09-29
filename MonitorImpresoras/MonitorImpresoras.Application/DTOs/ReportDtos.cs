namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para solicitud de generación de reporte
    /// </summary>
    public class ReportRequestDto
    {
        /// <summary>
        /// ID del template de reporte a generar
        /// </summary>
        public int ReportTemplateId { get; set; }

        /// <summary>
        /// Formato de exportación (json, csv, pdf, excel)
        /// </summary>
        public string Format { get; set; } = "json";

        /// <summary>
        /// Parámetros específicos para el reporte
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }

        /// <summary>
        /// Filtros adicionales para el reporte
        /// </summary>
        public ReportFilterDto? Filters { get; set; }

        /// <summary>
        /// Configuración personalizada del reporte
        /// </summary>
        public Dictionary<string, object>? Configuration { get; set; }
    }

    /// <summary>
    /// DTO para filtros de reporte
    /// </summary>
    public class ReportFilterDto
    {
        /// <summary>
        /// Fecha de inicio del período (opcional)
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Fecha de fin del período (opcional)
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Filtros específicos por campo
        /// </summary>
        public Dictionary<string, object>? FieldFilters { get; set; }

        /// <summary>
        /// IDs de entidades específicas a incluir
        /// </summary>
        public List<string>? IncludeIds { get; set; }

        /// <summary>
        /// IDs de entidades específicas a excluir
        /// </summary>
        public List<string>? ExcludeIds { get; set; }

        /// <summary>
        /// Número máximo de registros a incluir
        /// </summary>
        public int? MaxRecords { get; set; }
    }

    /// <summary>
    /// DTO para resultado de reporte
    /// </summary>
    public class ReportResultDto
    {
        /// <summary>
        /// ID único de la ejecución del reporte
        /// </summary>
        public int ExecutionId { get; set; }

        /// <summary>
        /// ID del template utilizado
        /// </summary>
        public int ReportTemplateId { get; set; }

        /// <summary>
        /// Nombre del reporte
        /// </summary>
        public string ReportName { get; set; } = default!;

        /// <summary>
        /// Formato del reporte generado
        /// </summary>
        public string Format { get; set; } = default!;

        /// <summary>
        /// Estado actual del reporte
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// Número de registros procesados
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Tamaño del archivo generado (bytes)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// URL para descargar el reporte
        /// </summary>
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// Fecha de inicio de generación
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Fecha de finalización (si aplica)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Tiempo de ejecución en segundos
        /// </summary>
        public double? ExecutionTimeSeconds { get; set; }

        /// <summary>
        /// Mensaje de error (si falló)
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO para template de reporte
    /// </summary>
    public class ReportTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string Category { get; set; } = default!;
        public string EntityType { get; set; } = default!;
        public string SupportedFormats { get; set; } = default!;
        public string? RequiredClaim { get; set; }
        public bool IsActive { get; set; }
        public int EstimatedExecutionTimeSeconds { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public string? CreatedByUserName { get; set; }
    }

    /// <summary>
    /// DTO para historial de reportes de un usuario
    /// </summary>
    public class ReportHistoryDto
    {
        public int ExecutionId { get; set; }
        public string ReportName { get; set; } = default!;
        public string Format { get; set; } = default!;
        public string Status { get; set; } = default!;
        public int RecordCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double? ExecutionTimeSeconds { get; set; }
        public string? DownloadUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO para reporte programado
    /// </summary>
    public class ScheduledReportDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int ReportTemplateId { get; set; }
        public string ReportTemplateName { get; set; } = default!;
        public string CronExpression { get; set; } = default!;
        public string Format { get; set; } = default!;
        public string? Recipients { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSuccessfulExecutionUtc { get; set; }
        public DateTime? NextExecutionUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    /// <summary>
    /// DTO para crear reporte programado
    /// </summary>
    public class CreateScheduledReportDto
    {
        public int ReportTemplateId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string CronExpression { get; set; } = default!;
        public string Format { get; set; } = "csv";
        public string? Recipients { get; set; }
        public Dictionary<string, object>? FixedParameters { get; set; }
    }

    /// <summary>
    /// DTO para actualizar reporte programado
    /// </summary>
    public class UpdateScheduledReportDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CronExpression { get; set; }
        public string? Format { get; set; }
        public string? Recipients { get; set; }
        public bool? IsActive { get; set; }
        public Dictionary<string, object>? FixedParameters { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de reportes
    /// </summary>
    public class ReportStatisticsDto
    {
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double AverageExecutionTimeSeconds { get; set; }
        public long TotalFileSizeBytes { get; set; }
        public DateTime? LastExecutionDate { get; set; }
        public Dictionary<string, int> ExecutionsByFormat { get; set; } = new();
        public Dictionary<string, int> ExecutionsByCategory { get; set; } = new();
    }
}
