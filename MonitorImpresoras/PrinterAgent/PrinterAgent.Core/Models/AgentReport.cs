namespace PrinterAgent.Core.Models
{
    /// <summary>
    /// Reporte que el agente envía al sistema central
    /// </summary>
    public class AgentReport
    {
        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public AgentHealthStatus Health { get; set; } = new();
        public List<PrinterInfo> Printers { get; set; } = new();
        public List<PrinterAlert> Alerts { get; set; } = new();
        public AgentMetrics Metrics { get; set; } = new();
    }

    public class AgentHealthStatus
    {
        public bool IsHealthy { get; set; }
        public TimeSpan Uptime { get; set; }
        public DateTime LastCentralCommunication { get; set; }
        public int PrintersMonitored { get; set; }
        public int ActiveAlerts { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double NetworkLatency { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    public class AgentMetrics
    {
        public int TotalPrintersDiscovered { get; set; }
        public int PrintersOnline { get; set; }
        public int PrintersOffline { get; set; }
        public int PrintersWithErrors { get; set; }
        public int TotalAlertsGenerated { get; set; }
        public int SuccessfulCommunications { get; set; }
        public int FailedCommunications { get; set; }
        public double AverageResponseTime { get; set; }
        public DateTime LastNetworkScan { get; set; }
        public TimeSpan NetworkScanDuration { get; set; }
    }

    /// <summary>
    /// Comando que el sistema central puede enviar al agente
    /// </summary>
    public class AgentCommand
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString();
        public string AgentId { get; set; } = string.Empty;
        public AgentCommandType Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public enum AgentCommandType
    {
        UpdateConfiguration = 0,
        ScanNetwork = 1,
        RestartAgent = 2,
        UpdateFirmware = 3,
        GenerateReport = 4,
        TestPrinter = 5,
        ClearAlerts = 6
    }

    /// <summary>
    /// Respuesta del agente a un comando
    /// </summary>
    public class AgentCommandResponse
    {
        public string CommandId { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}
