using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de trabajo de impresión
    /// </summary>
    public class PrintJob : BaseEntity
    {
        [Required]
        public Guid PrinterId { get; set; }

        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty; // Usuario que realizó la impresión

        [Required]
        [MaxLength(100)]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        public int Pages { get; set; }

        public int Copies { get; set; } = 1;

        [MaxLength(50)]
        public string Status { get; set; } = "Unknown";

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public bool IsColor { get; set; }

        public bool IsDuplex { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [MaxLength(100)]
        public string JobId { get; set; } = string.Empty; // ID del trabajo en el spooler

        // Navigation properties
        [ForeignKey("PrinterId")]
        public virtual Printer Printer { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
