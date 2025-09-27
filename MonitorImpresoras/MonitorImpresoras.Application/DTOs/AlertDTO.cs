namespace MonitorImpresoras.Application.DTOs
{
    public class AlertDTO
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
