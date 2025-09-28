using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Application.DTOs
{
    public class CreateAlertDTO
    {
        public int PrinterId { get; set; }
        public AlertType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }
}
