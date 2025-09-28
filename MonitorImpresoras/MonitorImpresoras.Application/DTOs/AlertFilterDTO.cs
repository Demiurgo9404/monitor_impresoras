namespace MonitorImpresoras.Application.DTOs
{
    public class AlertFilterDTO
    {
        public int? PrinterId { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
