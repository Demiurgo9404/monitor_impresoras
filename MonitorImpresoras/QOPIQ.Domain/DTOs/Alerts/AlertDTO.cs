namespace QOPIQ.Domain.DTOs.Alerts
{
    public class AlertDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public string Type { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public Guid? PrinterId { get; set; }
        public string? PrinterName { get; set; }
    }
}

