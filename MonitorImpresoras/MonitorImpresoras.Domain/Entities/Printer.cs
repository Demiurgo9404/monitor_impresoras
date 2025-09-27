using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de impresora
    /// </summary>
    public class Printer : BaseEntity
    {
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

        public int? PageCount { get; set; }

        public DateTime? LastMaintenance { get; set; }

        public int? MaintenanceIntervalDays { get; set; } = 90;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string LastError { get; set; } = string.Empty;

        public DateTime? LastChecked { get; set; }

        public DateTime? LastSeen { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public bool NeedsMaintenance => LastMaintenance == null ||
                                     (MaintenanceIntervalDays.HasValue &&
                                      LastMaintenance.Value.AddDays(MaintenanceIntervalDays.Value) <= DateTime.UtcNow);

        // Navigation properties
        public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
        public virtual ICollection<PrinterConsumable> ConsumableParts { get; set; } = new List<PrinterConsumable>();
    }
}
