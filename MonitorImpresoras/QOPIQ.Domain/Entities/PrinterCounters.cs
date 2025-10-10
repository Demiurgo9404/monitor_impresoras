using System;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents the counters and status information for a printer
    /// </summary>
    public class PrinterCounters
    {
        /// <summary>
        /// Total number of pages printed in color
        /// </summary>
        public int ColorPageCount { get; set; }

        /// <summary>
        /// Total number of pages printed in black and white
        /// </summary>
        public int BlackAndWhitePageCount { get; set; }

        /// <summary>
        /// Total number of copies made
        /// </summary>
        public int CopyCount { get; set; }

        /// <summary>
        /// Total number of scans performed
        /// </summary>
        public int ScanCount { get; set; }

        /// <summary>
        /// Total number of faxes sent
        /// </summary>
        public int FaxCount { get; set; }

        /// <summary>
        /// Total number of pages printed (color + black and white)
        /// </summary>
        public int TotalPageCount => ColorPageCount + BlackAndWhitePageCount;

        /// <summary>
        /// Counter for printer power cycles
        /// </summary>
        public int PowerCycleCount { get; set; }

        /// <summary>
        /// Counter for printer engine cycles
        /// </summary>
        public int EngineCycleCount { get; set; }

        /// <summary>
        /// Counter for printer uptime in seconds
        /// </summary>
        public long UptimeSeconds { get; set; }

        /// <summary>
        /// Counter for printer downtime in seconds
        /// </summary>
        public long DowntimeSeconds { get; set; }

        /// <summary>
        /// Counter for paper jams
        /// </summary>
        public int JamEvents { get; set; }

        /// <summary>
        /// Counter for paper out events
        /// </summary>
        public int PaperOutEvents { get; set; }

        /// <summary>
        /// Counter for toner low events
        /// </summary>
        public int TonerLowEvents { get; set; }

        /// <summary>
        /// Counter for toner empty events
        /// </summary>
        public int TonerEmptyEvents { get; set; }

        /// <summary>
        /// Counter for maintenance events
        /// </summary>
        public int MaintenanceCount { get; set; }

        /// <summary>
        /// Date and time when the counters were last reset
        /// </summary>
        public DateTime? LastResetDate { get; set; }

        /// <summary>
        /// Date and time when the counters were last updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
