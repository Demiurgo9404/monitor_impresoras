using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a report in the system
    /// </summary>
    public class Report : BaseEntity
    {
        /// <summary>
        /// Name of the report
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the report
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of the report
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Format of the report (PDF, Excel, etc.)
        /// </summary>
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Status of the report
        /// </summary>
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        /// <summary>
        /// Date and time when the report was generated
        /// </summary>
        public DateTime? GeneratedAt { get; set; }

        /// <summary>
        /// Path to the generated report file
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Size of the report file in bytes
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// MIME type of the report file
        /// </summary>
        public string? MimeType { get; set; }

        /// <summary>
        /// ID of the user who requested the report
        /// </summary>
        public string? RequestedBy { get; set; }

        /// <summary>
        /// ID of the tenant this report belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this report belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// Parameters used to generate the report (JSON format)
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// Error message if the report generation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Indicates if the report is a scheduled report
        /// </summary>
        public bool IsScheduled { get; set; }

        /// <summary>
        /// ID of the scheduled report that generated this report (if any)
        /// </summary>
        public Guid? ScheduledReportId { get; set; }

        /// <summary>
        /// The scheduled report that generated this report (if any)
        /// </summary>
        public virtual ScheduledReport? ScheduledReport { get; set; }
    }

    /// <summary>
    /// Represents the status of a report
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// Report generation is pending
        /// </summary>
        Pending,

        /// <summary>
        /// Report is being generated
        /// </summary>
        InProgress,

        /// <summary>
        /// Report has been generated successfully
        /// </summary>
        Completed,

        /// <summary>
        /// Report generation failed
        /// </summary>
        Failed,

        /// <summary>
        /// Report generation was cancelled
        /// </summary>
        Cancelled
    }
}
