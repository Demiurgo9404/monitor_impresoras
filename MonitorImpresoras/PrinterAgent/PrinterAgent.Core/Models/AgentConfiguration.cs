using System.ComponentModel.DataAnnotations;

namespace PrinterAgent.Core.Models
{
    /// <summary>
    /// Configuración del agente distribuido
    /// </summary>
    public class AgentConfiguration
    {
        [Required]
        public string AgentId { get; set; } = string.Empty;
        
        [Required]
        public string AgentName { get; set; } = string.Empty;
        
        public string Location { get; set; } = string.Empty;
        
        [Required]
        [Url]
        public string CentralApiUrl { get; set; } = string.Empty;
        
        [Required]
        public string ApiKey { get; set; } = string.Empty;
        
        public TimeSpan ReportingInterval { get; set; } = TimeSpan.FromMinutes(5);
        
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(1);
        
        public NetworkConfiguration Network { get; set; } = new();
        
        public LoggingConfiguration Logging { get; set; } = new();
    }

    public class NetworkConfiguration
    {
        public List<string> ScanRanges { get; set; } = new();
        
        public string SnmpCommunity { get; set; } = "public";
        
        public int SnmpTimeout { get; set; } = 5000;
        
        public int MaxConcurrentScans { get; set; } = 10;
        
        public bool EnableAutoDiscovery { get; set; } = true;
    }

    public class LoggingConfiguration
    {
        public string Level { get; set; } = "Information";
        
        public int RetentionDays { get; set; } = 30;
        
        public bool EnableFileLogging { get; set; } = true;
        
        public string LogPath { get; set; } = "logs";
    }
}
