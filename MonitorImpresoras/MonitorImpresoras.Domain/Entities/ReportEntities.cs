using MonitorImpresoras.Domain.Common;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para reportes
    /// </summary>
    public class Report : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public new DateTime CreatedAt { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FilterParameters { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long? FileSize { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// Entidad para plantillas de reportes
    /// </summary>
    public class ReportTemplate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public string TemplateContent { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
        public string DefaultConfiguration { get; set; } = string.Empty;
        public DateTime? DefaultDateFrom { get; set; }
        public DateTime? DefaultDateTo { get; set; }
        public int UsageCount { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<ScheduledReport> ScheduledReports { get; set; } = new List<ScheduledReport>();
    }

    /// <summary>
    /// Entidad para reportes programados
    /// </summary>
    public class ScheduledReport : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public ScheduleFrequency Frequency { get; set; }
        public DateTime NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Recipients { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
        public string ReportConfiguration { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int RetentionDays { get; set; } = 30;
        public bool SendOnFailure { get; set; } = true;

        // Navigation properties
        public virtual ReportTemplate Template { get; set; } = null!;
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
    }

    /// <summary>
    /// Entidad para ejecuciones de reportes
    /// </summary>
    public class ReportExecution : BaseEntity
    {
        public Guid ScheduledReportId { get; set; }
        public ExecutionStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public long? FileSizeBytes { get; set; }
        public int? RecordCount { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public List<string> EmailRecipients { get; set; } = new();
        public List<string> WebhookUrls { get; set; } = new();

        // Navigation properties
        public virtual ScheduledReport ScheduledReport { get; set; } = null!;
    }
}
