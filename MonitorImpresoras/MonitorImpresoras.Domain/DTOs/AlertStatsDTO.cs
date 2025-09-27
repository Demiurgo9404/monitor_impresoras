namespace MonitorImpresoras.Domain.DTOs
{
    /// <summary>
    /// DTO for alert statistics
    /// </summary>
    public class AlertStatsDTO
    {
        public int TotalAlerts { get; set; }
        public int OpenAlerts { get; set; }
        public int AcknowledgedAlerts { get; set; }
        public int ResolvedAlerts { get; set; }
        public int HighSeverityAlerts { get; set; }
        public int MediumSeverityAlerts { get; set; }
        public int LowSeverityAlerts { get; set; }
        public Dictionary<string, int> AlertsByType { get; set; } = new();
        public Dictionary<string, int> AlertsByStatus { get; set; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
    }
}
