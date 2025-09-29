using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad que define la estructura y configuración de un tipo de reporte
    /// </summary>
    [Table("ReportTemplates")]
    public class ReportTemplate
    {
        /// <summary>
        /// Identificador único del template
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre único del reporte
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Descripción del reporte
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Categoría del reporte (printers, users, audit, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = default!;

        /// <summary>
        /// Tipo de entidad sobre la que se genera el reporte
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = default!;

        /// <summary>
        /// Configuración JSON del reporte (columnas, filtros, etc.)
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public string Configuration { get; set; } = "{}";

        /// <summary>
        /// Parámetros requeridos para generar el reporte
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Parameters { get; set; }

        /// <summary>
        /// Formatos de exportación soportados (json, csv, pdf, excel)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string SupportedFormats { get; set; } = "json,csv";

        /// <summary>
        /// Claim requerido para acceder al reporte
        /// </summary>
        [MaxLength(200)]
        public string? RequiredClaim { get; set; }

        /// <summary>
        /// Indica si el reporte está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Tiempo máximo estimado para generar el reporte (en segundos)
        /// </summary>
        public int EstimatedExecutionTimeSeconds { get; set; } = 30;

        /// <summary>
        /// Fecha de creación del template
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Usuario que creó el template
        /// </summary>
        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Usuario que actualizó el template por última vez
        /// </summary>
        [MaxLength(450)]
        public string? UpdatedByUserId { get; set; }

        // Navegación
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
        public virtual ICollection<ScheduledReport> ScheduledReports { get; set; } = new List<ScheduledReport>();
    }

    /// <summary>
    /// Entidad que representa una ejecución histórica de un reporte
    /// </summary>
    [Table("ReportExecutions")]
    public class ReportExecution
    {
        /// <summary>
        /// Identificador único de la ejecución
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID del template utilizado
        /// </summary>
        [Required]
        public int ReportTemplateId { get; set; }

        /// <summary>
        /// Usuario que ejecutó el reporte
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string ExecutedByUserId { get; set; } = default!;

        /// <summary>
        /// Parámetros utilizados para generar el reporte
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Parameters { get; set; }

        /// <summary>
        /// Formato del reporte generado
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Format { get; set; } = "json";

        /// <summary>
        /// Estado de la ejecución
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, running, completed, failed

        /// <summary>
        /// Número de registros procesados
        /// </summary>
        public int RecordCount { get; set; } = 0;

        /// <summary>
        /// Tamaño del archivo generado (en bytes)
        /// </summary>
        public long FileSize { get; set; } = 0;

        /// <summary>
        /// Ruta del archivo generado (si aplica)
        /// </summary>
        [MaxLength(500)]
        public string? FilePath { get; set; }

        /// <summary>
        /// URL temporal para descarga
        /// </summary>
        [MaxLength(500)]
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// Fecha de inicio de la ejecución
        /// </summary>
        public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de finalización
        /// </summary>
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>
        /// Duración de la ejecución en segundos
        /// </summary>
        public double? ExecutionTimeSeconds { get; set; }

        /// <summary>
        /// Mensaje de error (si falló)
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// IP del usuario que ejecutó el reporte
        /// </summary>
        [MaxLength(45)]
        public string? ExecutedByIp { get; set; }

        // Navegación
        public virtual ReportTemplate ReportTemplate { get; set; } = default!;
        public virtual User ExecutedByUser { get; set; } = default!;
    }

    /// <summary>
    /// Entidad para reportes programados automáticos
    /// </summary>
    [Table("ScheduledReports")]
    public class ScheduledReport
    {
        /// <summary>
        /// Identificador único del reporte programado
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID del template a ejecutar
        /// </summary>
        [Required]
        public int ReportTemplateId { get; set; }

        /// <summary>
        /// Usuario que programó el reporte
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string CreatedByUserId { get; set; } = default!;

        /// <summary>
        /// Nombre del reporte programado
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Descripción del reporte programado
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Expresión cron para la programación
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CronExpression { get; set; } = "0 9 * * MON"; // Lunes 9:00 AM por defecto

        /// <summary>
        /// Formato de exportación
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Format { get; set; } = "csv";

        /// <summary>
        /// Emails de destinatarios (separados por coma)
        /// </summary>
        [MaxLength(1000)]
        public string? Recipients { get; set; }

        /// <summary>
        /// Parámetros fijos para el reporte
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? FixedParameters { get; set; }

        /// <summary>
        /// Indica si el reporte está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Última ejecución exitosa
        /// </summary>
        public DateTime? LastSuccessfulExecutionUtc { get; set; }

        /// <summary>
        /// Próxima ejecución programada
        /// </summary>
        public DateTime? NextExecutionUtc { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }

        // Navegación
        public virtual ReportTemplate ReportTemplate { get; set; } = default!;
        public virtual User CreatedByUser { get; set; } = default!;
        public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
    }
}
