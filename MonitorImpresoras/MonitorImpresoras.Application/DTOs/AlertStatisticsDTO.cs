namespace MonitorImpresoras.Application.DTOs
{
    public class AlertStatisticsDTO
    {
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int ResolvedAlerts { get; set; }
        public Dictionary<string, int> AlertsByType { get; set; } = new();
        public Dictionary<string, int> AlertsByStatus { get; set; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
    }
}
