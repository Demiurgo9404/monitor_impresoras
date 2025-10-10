using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents an execution of a report
    /// </summary>
    public class ReportExecution : BaseEntity
    {
        /// <summary>
        /// ID of the report that was executed
        /// </summary>
        public Guid ReportId { get; set; }

        /// <summary>
        /// Reference to the report
        /// </summary>
        public virtual Report Report { get; set; }

        /// <summary>
        /// ID of the user who executed the report
        /// </summary>
        public Guid? ExecutedBy { get; set; }

        /// <summary>
        /// Timestamp when the report was executed
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Status of the report execution
        /// </summary>
        public ReportExecutionStatus Status { get; set; } = ReportExecutionStatus.Pending;

        /// <summary>
        /// Path to the generated report file (if any)
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Format of the generated report
        /// </summary>
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Any error message if the execution failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Parameters used for this execution (serialized JSON)
        /// </summary>
        public string? Parameters { get; set; }
    }
}
