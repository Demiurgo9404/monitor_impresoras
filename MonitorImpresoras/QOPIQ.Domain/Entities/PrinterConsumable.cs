using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Entidad para consumibles de impresora
    /// </summary>
    public class PrinterConsumable : BaseEntity
    {
        [Required]
        public Guid PrinterId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PartName { get; set; } = string.Empty;

        [Required]
        public ConsumableType Type { get; set; }

        public int CurrentLevel { get; set; } // Nivel actual (0-100)

        public int? WarningLevel { get; set; } // Nivel para alerta

        public int? CriticalLevel { get; set; } // Nivel crítico

        public DateTime? LastUpdated { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("PrinterId")]
        public virtual Printer Printer { get; set; } = null!;
    }
}

