using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a subscription plan in the system
    /// </summary>
    public class Plan : BaseEntity
    {
        /// <summary>
        /// Name of the plan
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the plan
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The subscription level/tier
        /// </summary>
        public SubscriptionLevel Level { get; set; } = SubscriptionLevel.Free;

        /// <summary>
        /// Billing cycle for the plan
        /// </summary>
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

        /// <summary>
        /// Price per billing cycle
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Currency code (e.g., USD, EUR, MXN)
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Maximum number of users allowed
        /// </summary>
        public int? MaxUsers { get; set; }

        /// <summary>
        /// Maximum number of printers allowed
        /// </summary>
        public int? MaxPrinters { get; set; }

        /// <summary>
        /// Maximum number of print jobs per month
        /// </summary>
        public int? MaxPrintJobsPerMonth { get; set; }

        /// <summary>
        /// Maximum number of pages per month (black and white)
        /// </summary>
        public int? MaxPagesBW { get; set; }

        /// <summary>
        /// Maximum number of pages per month (color)
        /// </summary>
        public int? MaxPagesColor { get; set; }

        /// <summary>
        /// Cost per additional page (black and white)
        /// </summary>
        public decimal? CostPerPageBW { get; set; }

        /// <summary>
        /// Cost per additional page (color)
        /// </summary>
        public decimal? CostPerPageColor { get; set; }

        /// <summary>
        /// Indicates if the plan includes advanced reporting
        /// </summary>
        public bool HasAdvancedReporting { get; set; }

        /// <summary>
        /// Indicates if the plan includes API access
        /// </summary>
        public bool HasAPIAccess { get; set; }

        /// <summary>
        /// Indicates if the plan includes priority support
        /// </summary>
        public bool HasPrioritySupport { get; set; }

        /// <summary>
        /// Indicates if the plan is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date and time when the plan was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the plan was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// The subscriptions that use this plan
        /// </summary>
        public virtual ICollection<Subscription>? Subscriptions { get; set; }

        /// <summary>
        /// The features included in this plan
        /// </summary>
        public virtual ICollection<PlanFeature>? Features { get; set; }
    }

    /// <summary>
    /// Represents a feature included in a plan
    /// </summary>
    public class PlanFeature : BaseEntity
    {
        /// <summary>
        /// ID of the plan this feature belongs to
        /// </summary>
        public Guid PlanId { get; set; }

        /// <summary>
        /// The plan this feature belongs to
        /// </summary>
        public virtual Plan? Plan { get; set; }

        /// <summary>
        /// Name of the feature
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the feature
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Value of the feature (e.g., "5GB", "Unlimited", "24/7")
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Display order of the feature
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
