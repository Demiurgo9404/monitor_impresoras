using System;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a subscription in the system
    /// </summary>
    public class Subscription : BaseEntity
    {
        /// <summary>
        /// Name of the subscription plan
        /// </summary>
        public string PlanName { get; set; } = string.Empty;

        /// <summary>
        /// Description of the subscription
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Status of the subscription
        /// </summary>
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        /// <summary>
        /// Billing cycle for the subscription
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
        /// Date when the subscription starts
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the subscription ends (for fixed-term subscriptions)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Date of the next billing cycle
        /// </summary>
        public DateTime? NextBillingDate { get; set; }

        /// <summary>
        /// Date when the subscription was canceled
        /// </summary>
        public DateTime? CanceledDate { get; set; }

        /// <summary>
        /// Reason for cancellation
        /// </summary>
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Indicates if the subscription will automatically renew
        /// </summary>
        public bool AutoRenew { get; set; } = true;

        /// <summary>
        /// Maximum number of users allowed in this subscription
        /// </summary>
        public int MaxUsers { get; set; } = 1;

        /// <summary>
        /// Maximum number of printers allowed in this subscription
        /// </summary>
        public int MaxPrinters { get; set; } = 1;

        /// <summary>
        /// Indicates if the subscription includes advanced reporting
        /// </summary>
        public bool IncludesAdvancedReporting { get; set; }

        /// <summary>
        /// Indicates if the subscription includes priority support
        /// </summary>
        public bool IncludesPrioritySupport { get; set; }

        /// <summary>
        /// Indicates if the subscription is a trial
        /// </summary>
        public bool IsTrial { get; set; }

        /// <summary>
        /// Date when the trial ends (if applicable)
        /// </summary>
        public DateTime? TrialEndDate { get; set; }

        /// <summary>
        /// External subscription ID (e.g., from payment processor)
        /// </summary>
        public string? ExternalSubscriptionId { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
