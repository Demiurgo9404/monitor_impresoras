namespace MonitorImpresoras.Application.DTOs
{
    public class AlertDTO
    {
        public Guid Id { get; set; }
        public int PrinterId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DetectedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
