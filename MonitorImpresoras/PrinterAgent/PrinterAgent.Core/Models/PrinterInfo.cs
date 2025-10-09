namespace PrinterAgent.Core.Models
{
    /// <summary>
    /// Información de una impresora detectada por el agente
    /// </summary>
    public class PrinterInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        public PrinterStatus Status { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime FirstDetected { get; set; }
        public PrinterCapabilities Capabilities { get; set; } = new();
        public PrinterMetrics Metrics { get; set; } = new();
        public List<PrinterAlert> Alerts { get; set; } = new();
    }

    public enum PrinterStatus
    {
        Unknown = 0,
        Online = 1,
        Offline = 2,
        Error = 3,
        Maintenance = 4,
        Sleeping = 5
    }

    public class PrinterCapabilities
    {
        public bool SupportsSnmp { get; set; }
        public bool SupportsColor { get; set; }
        public bool SupportsDuplex { get; set; }
        public bool SupportsWifi { get; set; }
        public List<string> SupportedMediaTypes { get; set; } = new();
        public List<string> SupportedMediaSizes { get; set; } = new();
    }

    public class PrinterMetrics
    {
        public int TotalPageCount { get; set; }
        public int PagesSinceLastReport { get; set; }
        public int JobsInQueue { get; set; }
        public ConsumableLevel BlackToner { get; set; } = new();
        public ConsumableLevel CyanToner { get; set; } = new();
        public ConsumableLevel MagentaToner { get; set; } = new();
        public ConsumableLevel YellowToner { get; set; } = new();
        public int? Temperature { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ConsumableLevel
    {
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public double PercentageRemaining => MaxLevel > 0 ? (double)CurrentLevel / MaxLevel * 100 : 0;
        public bool IsLow => PercentageRemaining < 20;
        public bool IsCritical => PercentageRemaining < 10;
    }

    public class PrinterAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public enum AlertSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }
}

