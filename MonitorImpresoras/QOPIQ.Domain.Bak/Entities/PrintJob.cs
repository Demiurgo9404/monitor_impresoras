using System;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a print job in the system
    /// </summary>
    public class PrintJob : BaseEntity
    {
        /// <summary>
        /// Name of the print job
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Status of the print job
        /// </summary>
        public PrintJobStatus Status { get; set; } = PrintJobStatus.Queued;

        /// <summary>
        /// Number of pages in the print job
        /// </summary>
        public int PageCount { get; set; } = 1;

        /// <summary>
        /// Number of copies
        /// </summary>
        public int Copies { get; set; } = 1;

        /// <summary>
        /// Indicates if the print job is in color
        /// </summary>
        public bool IsColor { get; set; }

        /// <summary>
        /// Indicates if the print job is double-sided
        /// </summary>
        public bool IsDuplex { get; set; }

        /// <summary>
        /// Size of the print job in bytes
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Date and time when the print job was submitted
        /// </summary>
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the print job started processing
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Date and time when the print job was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Error message if the print job failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Foreign key to the associated printer
        /// </summary>
        public Guid PrinterId { get; set; }

        /// <summary>
        /// Navigation property to the associated printer
        /// </summary>
        public virtual Printer? Printer { get; set; }

        /// <summary>
        /// Foreign key to the user who submitted the print job
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Navigation property to the user who submitted the print job
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
