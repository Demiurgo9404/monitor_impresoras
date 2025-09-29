using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = default!;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = default!;

        [Required]
        [MaxLength(100)]
        public string Entity { get; set; } = default!;

        [MaxLength(100)]
        public string? EntityId { get; set; }

        [MaxLength(500)]
        public string? Details { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navegación
        public virtual User? User { get; set; }
    }
}
