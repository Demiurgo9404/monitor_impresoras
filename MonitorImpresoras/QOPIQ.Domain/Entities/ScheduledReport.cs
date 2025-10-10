using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a scheduled report in the system
    /// </summary>
    public class ScheduledReport : BaseEntity
    {
        /// <summary>
        /// Name of the scheduled report
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the scheduled report
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// ID of the tenant this scheduled report belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this scheduled report belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// ID of the report template this scheduled report uses
        /// </summary>
        public int ReportTemplateId { get; set; }

        /// <summary>
        /// The report template this scheduled report uses
        /// </summary>
        public virtual ReportTemplate? ReportTemplate { get; set; }

        /// <summary>
        /// Schedule frequency (daily, weekly, monthly, etc.)
        /// </summary>
        public ScheduleFrequency Frequency { get; set; }

        /// <summary>
        /// Schedule configuration (cron expression or similar)
        /// </summary>
        public string Schedule { get; set; } = string.Empty;

        /// <summary>
        /// Email addresses to send the report to (comma-separated)
        /// </summary>
        public string? EmailRecipients { get; set; }

        /// <summary>
        /// Whether the scheduled report is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Last time the report was executed
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        /// Next scheduled run time
        /// </summary>
        public DateTime? NextRunTime { get; set; }

        /// <summary>
        /// Parameters for the report (JSON format)
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// Format of the report (PDF, Excel, etc.)
        /// </summary>
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Time zone ID for scheduling
        /// </summary>
        public string TimeZoneId { get; set; } = "UTC";

        /// <summary>
        /// User who created the scheduled report
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// User who last modified the scheduled report
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Date and time the scheduled report was last modified
        /// </summary>
        public DateTime? ModifiedAt { get; set; }
    }
}
