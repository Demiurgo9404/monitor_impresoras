using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Status of a print job
    /// </summary>
    public enum PrintJobStatus
    {
        /// <summary>
        /// Job is queued and waiting to be processed
        /// </summary>
        [Description("Queued")]
        Queued = 0,

        /// <summary>
        /// Job is currently being processed
        /// </summary>
        [Description("Processing")]
        Processing = 1,

        /// <summary>
        /// Job is being sent to the printer
        /// </summary>
        [Description("Spooling")]
        Spooling = 2,

        /// <summary>
        /// Job is currently printing
        /// </summary>
        [Description("Printing")]
        Printing = 3,

        /// <summary>
        /// Job has been paused
        /// </summary>
        [Description("Paused")]
        Paused = 4,

        /// <summary>
        /// Job has been canceled
        /// </summary>
        [Description("Canceled")]
        Canceled = 5,

        /// <summary>
        /// Job has completed successfully
        /// </summary>
        [Description("Completed")]
        Completed = 6,

        /// <summary>
        /// Job has failed
        /// </summary>
        [Description("Failed")]
        Failed = 7,

        /// <summary>
        /// Job has been deleted
        /// </summary>
        [Description("Deleted")]
        Deleted = 8,

        /// <summary>
        /// Job is blocked and cannot be processed
        /// </summary>
        [Description("Blocked")]
        Blocked = 9,

        /// <summary>
        /// Job is in an unknown state
        /// </summary>
        [Description("Unknown")]
        Unknown = 99
    }
}
