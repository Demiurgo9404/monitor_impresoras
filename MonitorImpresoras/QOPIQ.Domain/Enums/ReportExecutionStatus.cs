namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Represents the status of a report execution
    /// </summary>
    public enum ReportExecutionStatus
    {
        /// <summary>
        /// Report execution is pending
        /// </summary>
        Pending,

        /// <summary>
        /// Report is currently being generated
        /// </summary>
        InProgress,

        /// <summary>
        /// Report was generated successfully
        /// </summary>
        Completed,

        /// <summary>
        /// Report generation failed
        /// </summary>
        Failed,

        /// <summary>
        /// Report execution was canceled
        /// </summary>
        Canceled
    }
}
