using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.DTOs
{
    /// <summary>
    /// DTO para información de ejecución de reporte
    /// </summary>
    public class ReportExecutionDTO
    {
        public int Id { get; set; }
        public int ReportTemplateId { get; set; }
        public int ScheduledReportId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ExecutionStatus Status { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string ErrorDetails { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long? FileSizeBytes { get; set; }
        public int? RecordCount { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public bool EmailSent { get; set; }
        public bool WebhookSent { get; set; }
        public List<string> SentToEmails { get; set; } = new();
        public List<string> SentToWebhooks { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}

