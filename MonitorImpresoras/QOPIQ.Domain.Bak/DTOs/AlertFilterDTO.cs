namespace QOPIQ.Domain.DTOs
{
    /// <summary>
    /// DTO for filtering alerts
    /// </summary>
    public class AlertFilterDTO
    {
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Guid? PrinterId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

