using System;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a user's printing quota
    /// </summary>
    public class UserQuota : BaseEntity
    {
        /// <summary>
        /// ID of the user this quota belongs to
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The user this quota belongs to
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Maximum number of pages allowed (monochrome)
        /// </summary>
        public int? MaxPagesBW { get; set; }

        /// <summary>
        /// Maximum number of pages allowed (color)
        /// </summary>
        public int? MaxPagesColor { get; set; }

        /// <summary>
        /// Current page count (monochrome)
        /// </summary>
        public int CurrentPagesBW { get; set; } = 0;

        /// <summary>
        /// Current page count (color)
        /// </summary>
        public int CurrentPagesColor { get; set; } = 0;

        /// <summary>
        /// Quota period (e.g., Monthly, Quarterly, Yearly)
        /// </summary>
        public string QuotaPeriod { get; set; } = "Monthly";

        /// <summary>
        /// Date when the quota period starts
        /// </summary>
        public DateTime PeriodStartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the quota period ends
        /// </summary>
        public DateTime PeriodEndDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        /// <summary>
        /// Indicates if the quota is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date and time when the quota was last reset
        /// </summary>
        public DateTime? LastResetDate { get; set; }

        /// <summary>
        /// Notes or comments about the quota
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID of the tenant this quota belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this quota belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }
    }
}
