using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Represents the status of a report
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// Report is pending generation
        /// </summary>
        [Description("Pending")]
        Pending = 0,

        /// <summary>
        /// Report is being generated
        /// </summary>
        [Description("Processing")]
        Processing = 1,

        /// <summary>
        /// Report has been generated successfully
        /// </summary>
        [Description("Completed")]
        Completed = 2,

        /// <summary>
        /// Report generation failed
        /// </summary>
        [Description("Failed")]
        Failed = 3,

        /// <summary>
        /// Report generation was cancelled
        /// </summary>
        [Description("Cancelled")]
        Cancelled = 4
    }
}
