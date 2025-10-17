using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;
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
        /// The subscription level/tier
        /// </summary>
        public QOPIQ.Domain.Enums.SubscriptionLevel Level { get; set; } = QOPIQ.Domain.Enums.SubscriptionLevel.Free;

        /// <summary>
        /// Status of the subscription
        /// </summary>
        public QOPIQ.Domain.Enums.SubscriptionStatus Status { get; set; } = QOPIQ.Domain.Enums.SubscriptionStatus.Active;

        /// <summary>
        /// Billing cycle for the subscription
        /// </summary>
        public QOPIQ.Domain.Enums.BillingCycle BillingCycle { get; set; } = QOPIQ.Domain.Enums.BillingCycle.Monthly;

        /// <summary>
        /// Price per billing cycle
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The subscription plan details (alias for PlanName for backward compatibility)
        /// </summary>
        public string Plan { get => PlanName; set => PlanName = value; }

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
        /// Stripe subscription ID
        /// </summary>
        public string? StripeSubscriptionId { get; set; }

        /// <summary>
        /// User ID for the subscription owner
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Collection of invoices for this subscription
        /// </summary>
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
