using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para eventos del sistema de auditoría extendida
    /// </summary>
    [Table("SystemEvents")]
    public class SystemEvent
    {
        /// <summary>
        /// Identificador único del evento
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Tipo de evento (report_generated, report_failed, email_sent, etc.)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = default!;

        /// <summary>
        /// Categoría del evento (reports, emails, security, system)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = default!;

        /// <summary>
        /// Nivel de severidad (Info, Warning, Error, Critical)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = "Info";

        /// <summary>
        /// Título descriptivo del evento
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        /// <summary>
        /// Descripción detallada del evento
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Datos adicionales del evento en formato JSON
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? EventData { get; set; }

        /// <summary>
        /// Usuario relacionado con el evento (si aplica)
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// IP del usuario que generó el evento
        /// </summary>
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent del navegador/cliente
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// ID de la sesión del usuario
        /// </summary>
        [MaxLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// ID de la solicitud HTTP (para correlación)
        /// </summary>
        [MaxLength(100)]
        public string? RequestId { get; set; }

        /// <summary>
        /// Endpoint HTTP relacionado (si aplica)
        /// </summary>
        [MaxLength(500)]
        public string? Endpoint { get; set; }

        /// <summary>
        /// Método HTTP (GET, POST, etc.)
        /// </summary>
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        /// <summary>
        /// Código de estado HTTP (si aplica)
        /// </summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// Tiempo de ejecución en milisegundos
        /// </summary>
        public long? ExecutionTimeMs { get; set; }

        /// <summary>
        /// Indica si el evento fue exitoso o falló
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Mensaje de error (si falló)
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Stack trace del error (si aplica)
        /// </summary>
        [MaxLength(4000)]
        public string? StackTrace { get; set; }

        /// <summary>
        /// Información adicional del entorno
        /// </summary>
        [MaxLength(1000)]
        public string? EnvironmentInfo { get; set; }

        /// <summary>
        /// Versión de la aplicación
        /// </summary>
        [MaxLength(50)]
        public string? ApplicationVersion { get; set; }

        /// <summary>
        /// Nombre del servidor
        /// </summary>
        [MaxLength(100)]
        public string? ServerName { get; set; }

        /// <summary>
        /// Timestamp del evento en UTC
        /// </summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navegación
        public virtual User? User { get; set; }
    }
}
