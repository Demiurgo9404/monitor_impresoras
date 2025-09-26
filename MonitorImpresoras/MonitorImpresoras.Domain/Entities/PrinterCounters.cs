using System.Collections.Generic;
using MonitorImpresoras.Domain.Common;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// DTO para contadores de impresora obtenidos via SNMP
    /// </summary>
    public class PrinterCounters
    {
        public Guid PrinterId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public Dictionary<string, object> CounterValues { get; set; } = new();
        public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;

        // Contadores comunes de impresoras
        public int? TotalPagesPrinted { get; set; }
        public int? BlackPagesPrinted { get; set; }
        public int? ColorPagesPrinted { get; set; }
        public int? TotalCopies { get; set; }
        public int? TotalScans { get; set; }
        public int? TotalFaxes { get; set; }

        // Estados de consumibles
        public int? BlackTonerLevel { get; set; }
        public int? CyanTonerLevel { get; set; }
        public int? MagentaTonerLevel { get; set; }
        public int? YellowTonerLevel { get; set; }
        public int? DrumLevel { get; set; }
        public int? FuserLevel { get; set; }
        public int? TransferBeltLevel { get; set; }

        // Estados de la impresora
        public PrinterStatus Status { get; set; } = PrinterStatus.Unknown;
        public string StatusMessage { get; set; } = string.Empty;
        public int? ErrorCode { get; set; }
        public List<string> ActiveAlerts { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();

        // Información de identificación
        public string ModelName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Partes consumibles de la impresora
    /// </summary>
    public class PrinterConsumablePart : BaseEntity
    {
        public Guid PrinterId { get; set; }
        public ConsumableType ConsumableType { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public int MaxCapacity { get; set; }
        public int WarningThreshold { get; set; } = 20;
        public int CriticalThreshold { get; set; } = 10;
        public DateTime? LastReplaced { get; set; }
        public int EstimatedPagesRemaining { get; set; }
        public bool IsOriginal { get; set; } = true;
        public string SerialNumber { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Supplier { get; set; } = string.Empty;
    }
}
