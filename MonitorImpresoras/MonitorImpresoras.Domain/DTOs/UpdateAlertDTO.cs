namespace MonitorImpresoras.Domain.DTOs
{
    /// <summary>
    /// DTO for updating an alert
    /// </summary>
    public class UpdateAlertDTO
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}
