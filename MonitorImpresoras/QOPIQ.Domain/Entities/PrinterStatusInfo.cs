using System;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents the status information of a printer
    /// </summary>
    public class PrinterStatusInfo
    {
        /// <summary>
        /// Name of the printer
        /// </summary>
        public string PrinterName { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the printer
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the printer is online
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Indicates if the printer is ready to print
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Indicates if the printer is out of paper
        /// </summary>
        public bool IsOutOfPaper { get; set; }

        /// <summary>
        /// Indicates if the printer is in an error state
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Description of the current error (if any)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of print jobs in the queue
        /// </summary>
        public int JobsQueued { get; set; }

        /// <summary>
        /// Timestamp of the last status update
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional status information
        /// </summary>
        public Dictionary<string, string>? ExtendedStatus { get; set; }
    }
}
