using System;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents monitoring statistics for the system
    /// </summary>
    public class MonitoringStats : BaseEntity
    {
        /// <summary>
        /// ID of the tenant these stats belong to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant these stats belong to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// Total number of printers being monitored
        /// </summary>
        public int TotalPrinters { get; set; }

        /// <summary>
        /// Number of printers currently online
        /// </summary>
        public int PrintersOnline { get; set; }

        /// <summary>
        /// Number of printers currently offline
        /// </summary>
        public int PrintersOffline { get; set; }

        /// <summary>
        /// Number of printers with warnings
        /// </summary>
        public int PrintersWithWarnings { get; set; }

        /// <summary>
        /// Number of printers with errors
        /// </summary>
        public int PrintersWithErrors { get; set; }

        /// <summary>
        /// Total number of print jobs processed
        /// </summary>
        public int TotalPrintJobs { get; set; }

        /// <summary>
        /// Number of print jobs today
        /// </summary>
        public int PrintJobsToday { get; set; }

        /// <summary>
        /// Number of print jobs this week
        /// </summary>
        public int PrintJobsThisWeek { get; set; }

        /// <summary>
        /// Number of print jobs this month
        /// </summary>
        public int PrintJobsThisMonth { get; set; }

        /// <summary>
        /// Total pages printed
        /// </summary>
        public long TotalPagesPrinted { get; set; }

        /// <summary>
        /// Total color pages printed
        /// </summary>
        public long ColorPagesPrinted { get; set; }

        /// <summary>
        /// Total black and white pages printed
        /// </summary>
        public long BlackAndWhitePagesPrinted { get; set; }

        /// <summary>
        /// Total cost of printing
        /// </summary>
        public decimal TotalPrintingCost { get; set; }

        /// <summary>
        /// Date and time when these stats were last updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
