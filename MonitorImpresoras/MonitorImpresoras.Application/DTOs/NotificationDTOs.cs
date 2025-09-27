using System;

namespace MonitorImpresoras.Application.DTOs
{
    public class PrinterStatusUpdateDTO
    {
        public int PrinterId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
    }

    public class ConsumableAlertDTO
    {
        public int PrinterId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public int ConsumableId { get; set; }
        public string ConsumableName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? CurrentLevel { get; set; }
        public int? MaxCapacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
    }

    public class MonitoringStatusDTO
    {
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ActiveConnections { get; set; }
        public int MonitoredPrinters { get; set; }
    }
}
