using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a historical record of subscription changes
    /// </summary>
    public class SubscriptionHistory : BaseEntity
    {
        /// <summary>
        /// ID of the subscription this history entry is for
        /// </summary>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// The subscription this history entry is for
        /// </summary>
        public virtual Subscription? Subscription { get; set; }

        /// <summary>
        /// Type of change that occurred
        /// </summary>
        public SubscriptionChangeType ChangeType { get; set; }

        /// <summary>
        /// Description of the change
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Previous subscription level (if changed)
        /// </summary>
        public SubscriptionLevel? PreviousLevel { get; set; }

        /// <summary>
        /// New subscription level (if changed)
        /// </summary>
        public SubscriptionLevel? NewLevel { get; set; }

        /// <summary>
        /// Previous subscription status (if changed)
        /// </summary>
        public SubscriptionStatus? PreviousStatus { get; set; }

        /// <summary>
        /// New subscription status (if changed)
        /// </summary>
        public SubscriptionStatus? NewStatus { get; set; }

        /// <summary>
        /// Previous billing cycle (if changed)
        /// </summary>
        public BillingCycle? PreviousBillingCycle { get; set; }

        /// <summary>
        /// New billing cycle (if changed)
        /// </summary>
        public BillingCycle? NewBillingCycle { get; set; }

        /// <summary>
        /// Previous price (if changed)
        /// </summary>
        public decimal? PreviousPrice { get; set; }

        /// <summary>
        /// New price (if changed)
        /// </summary>
        public decimal? NewPrice { get; set; }

        /// <summary>
        /// ID of the user who made the change
        /// </summary>
        public Guid? ChangedBy { get; set; }

        /// <summary>
        /// Date and time when the change was made
        /// </summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional metadata about the change (stored as JSON)
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// ID of the tenant this history entry belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this history entry belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }
    }

    /// <summary>
    /// Represents the type of change made to a subscription
    /// </summary>
    public enum SubscriptionChangeType
    {
        /// <summary>
        /// Subscription was created
        /// </summary>
        Created,

        /// <summary>
        /// Subscription level was changed
        /// </summary>
        LevelChanged,

        /// <summary>
        /// Subscription status was changed
        /// </summary>
        StatusChanged,

        /// <summary>
        /// Billing cycle was changed
        /// </summary>
        BillingCycleChanged,

        /// <summary>
        /// Price was changed
        /// </summary>
        PriceChanged,

        /// <summary>
        /// Subscription was renewed
        /// </summary>
        Renewed,

        /// <summary>
        /// Subscription was canceled
        /// </summary>
        Canceled,

        /// <summary>
        /// Subscription was reactivated
        /// </summary>
        Reactivated,

        /// <summary>
        /// Other type of change
        /// </summary>
        Other
    }
}
