using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de impresora - QOPIQ Multi-Tenant
    /// </summary>
    public class Printer : BaseEntity
    {
        // Multi-tenant support
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Unknown";

        public bool IsOnline { get; set; }

        public bool IsLocalPrinter { get; set; }

        [MaxLength(50)]
        public string CommunityString { get; set; } = "public";

        public int? SnmpPort { get; set; } = 161;

        // Niveles de tóner y tinta
        public int? BlackInkLevel { get; set; }
        public int? CyanInkLevel { get; set; }
        public int? MagentaInkLevel { get; set; }
        public int? YellowInkLevel { get; set; }
        public int? BlackTonerLevel { get; set; }
        public int? CyanTonerLevel { get; set; }
        public int? MagentaTonerLevel { get; set; }
        public int? YellowTonerLevel { get; set; }

        // Contadores específicos QOPIQ
        public int? PageCount { get; set; }
        public int? TotalPagesPrinted { get; set; }
        public int? TotalPrintsBlack { get; set; }
        public int? TotalPrintsColor { get; set; }
        public int? TotalCopies { get; set; }
        public int? TotalScans { get; set; }
        
        // Contadores de scanner específicos
        public int? ScannerCounterBW { get; set; }
        public int? ScannerCounterColor { get; set; }
        public int? ScannerCounterTotal { get; set; }
        
        // Contadores de impresión detallados
        public int? PrintCounterA4BW { get; set; }
        public int? PrintCounterA4Color { get; set; }
        public int? PrintCounterA3BW { get; set; }
        public int? PrintCounterA3Color { get; set; }
        
        // Estado del fusor
        public int? FuserLevel { get; set; }
        public int? FuserLifeRemaining { get; set; }
        public bool? FuserNeedsReplacement { get; set; }
        
        // Drum/Tambor
        public int? DrumLevel { get; set; }
        public int? DrumLifeRemaining { get; set; }
        
        // Waste Toner Box
        public int? WasteTonerLevel { get; set; }

        // Mantenimiento
        public DateTime? LastMaintenance { get; set; }
        public int? MaintenanceIntervalDays { get; set; } = 90;
        public int? DaysUntilMaintenance => LastMaintenance.HasValue ? 
            MaintenanceIntervalDays - (int)(DateTime.UtcNow - LastMaintenance.Value).TotalDays : null;

        // Estado y errores
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string LastError { get; set; } = string.Empty;

        public DateTime? LastChecked { get; set; }
        public DateTime? LastSeen { get; set; } = DateTime.UtcNow;
        
        // Alertas
        public bool LowTonerWarning { get; set; }
        public bool LowInkWarning { get; set; }
        public bool PaperJam { get; set; }
        public bool NeedsUserAttention { get; set; }

        [NotMapped]
        public bool NeedsMaintenance => LastMaintenance == null ||
                                     (MaintenanceIntervalDays.HasValue &&
                                      LastMaintenance.Value.AddDays(MaintenanceIntervalDays.Value) <= DateTime.UtcNow);

        // Navigation properties
        public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
        public virtual ICollection<PrinterConsumable> ConsumableParts { get; set; } = new List<PrinterConsumable>();
    }
}
