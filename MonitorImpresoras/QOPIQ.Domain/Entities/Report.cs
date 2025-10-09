using QOPIQ.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Entidad de reporte automático - QOPIQ
    /// </summary>
    public class Report : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ReportType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Custom

        public DateTime ReportPeriodStart { get; set; }
        public DateTime ReportPeriodEnd { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Generated"; // Generated, Sent, Failed

        // Datos del reporte
        public int TotalPrinters { get; set; }
        public int ActivePrinters { get; set; }
        public int InactivePrinters { get; set; }
        public int PrintersWithErrors { get; set; }

        // Contadores totales
        public long TotalPrintsBW { get; set; }
        public long TotalPrintsColor { get; set; }
        public long TotalScans { get; set; }
        public long TotalCopies { get; set; }

        // Consumibles
        public int PrintersLowToner { get; set; }
        public int PrintersLowFuser { get; set; }
        public int PrintersNeedMaintenance { get; set; }

        // Costos (opcional)
        public decimal? TotalCostBW { get; set; }
        public decimal? TotalCostColor { get; set; }
        public decimal? TotalMaintenanceCost { get; set; }

        // Archivo del reporte
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string FileFormat { get; set; } = "PDF"; // PDF, Excel, CSV

        public long FileSizeBytes { get; set; }

        // Envío por email
        public bool EmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        
        [MaxLength(500)]
        public string EmailRecipients { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<ReportDetail> ReportDetails { get; set; } = new List<ReportDetail>();
    }

    /// <summary>
    /// Detalle del reporte por impresora
    /// </summary>
    public class ReportDetail : BaseEntity
    {
        [Required]
        public Guid ReportId { get; set; }

        [Required]
        public Guid PrinterId { get; set; }

        [MaxLength(100)]
        public string PrinterName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string PrinterModel { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        // Contadores del período
        public int PrintsBW { get; set; }
        public int PrintsColor { get; set; }
        public int Scans { get; set; }
        public int Copies { get; set; }

        // Estado de consumibles
        public int? TonerBlackLevel { get; set; }
        public int? TonerCyanLevel { get; set; }
        public int? TonerMagentaLevel { get; set; }
        public int? TonerYellowLevel { get; set; }
        public int? FuserLevel { get; set; }
        public int? DrumLevel { get; set; }

        // Tiempo de actividad
        public double UptimePercentage { get; set; }
        public int ErrorCount { get; set; }

        [MaxLength(500)]
        public string Observations { get; set; } = string.Empty;

        // Navigation properties
        public virtual Report Report { get; set; } = null!;
        public virtual Printer Printer { get; set; } = null!;
    }
}

