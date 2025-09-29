using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para rastrear el historial de escalamiento de notificaciones
    /// </summary>
    [Table("NotificationEscalationHistory")]
    public class NotificationEscalationHistory
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID único de la notificación original
        /// </summary>
        [Required, MaxLength(36)]
        public string NotificationId { get; set; } = default!;

        /// <summary>
        /// Nivel de escalamiento (1 = primera notificación, 2 = primer escalamiento, etc.)
        /// </summary>
        [Required]
        public int EscalationLevel { get; set; }

        /// <summary>
        /// Canal de notificación usado
        /// </summary>
        [Required, MaxLength(50)]
        public string Channel { get; set; } = default!;

        /// <summary>
        /// Destinatarios originales de la notificación
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public string OriginalRecipients { get; set; } = default!;

        /// <summary>
        /// Destinatarios del escalamiento (pueden ser diferentes)
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public string EscalatedRecipients { get; set; } = default!;

        /// <summary>
        /// Motivo del escalamiento
        /// </summary>
        [Required, MaxLength(200)]
        public string EscalationReason { get; set; } = default!;

        /// <summary>
        /// Tiempo máximo de respuesta esperado antes del escalamiento
        /// </summary>
        [Required]
        public int ResponseTimeMinutes { get; set; }

        /// <summary>
        /// Fecha y hora en que se envió la notificación original
        /// </summary>
        [Required]
        public DateTime OriginalNotificationSentAt { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó el escalamiento
        /// </summary>
        [Required]
        public DateTime EscalatedAt { get; set; }

        /// <summary>
        /// Usuario que reconoció la alerta (si aplica)
        /// </summary>
        [MaxLength(450)]
        public string? AcknowledgedBy { get; set; }

        /// <summary>
        /// Fecha y hora en que se reconoció la alerta (si aplica)
        /// </summary>
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// Comentarios del reconocimiento (si aplica)
        /// </summary>
        [MaxLength(1000)]
        public string? AcknowledgmentComments { get; set; }

        /// <summary>
        /// Si el escalamiento fue exitoso
        /// </summary>
        public bool EscalationSuccessful { get; set; }

        /// <summary>
        /// Mensaje de error si el escalamiento falló
        /// </summary>
        [MaxLength(1000)]
        public string? EscalationErrorMessage { get; set; }

        /// <summary>
        /// Metadata adicional del escalamiento
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? EscalationMetadata { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que creó el registro
        /// </summary>
        [MaxLength(450)]
        public string CreatedBy { get; set; } = "System";

        /// <summary>
        /// Última modificación del registro
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Usuario que modificó el registro
        /// </summary>
        [MaxLength(450)]
        public string? ModifiedBy { get; set; }
    }
}
