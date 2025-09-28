using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Application.DTOs
{
    public class UpdateAlertDTO
    {
        public string? Description { get; set; }
        public AlertSeverity? Severity { get; set; }
        public AlertStatus? Status { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
