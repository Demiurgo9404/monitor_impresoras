using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a tenant in the multi-tenant system
    /// </summary>
    public class Tenant : BaseEntity
    {
        /// <summary>
        /// Unique identifier for the tenant (used in the connection string)
        /// </summary>
        public string TenantKey { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the tenant
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Company name (if applicable)
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Contact email for the tenant
        /// </summary>
        public string AdminEmail { get; set; } = string.Empty;

        /// <summary>
        /// Subscription tier/level
        /// </summary>
        public SubscriptionLevel SubscriptionTier { get; set; } = SubscriptionLevel.Free;

        /// <summary>
        /// Date and time the tenant was created
        /// </summary>
        public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the tenant is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Connection string for the tenant's database
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Date and time the subscription expires
        /// </summary>
        public DateTime? SubscriptionExpiresAt { get; set; }

        /// <summary>
        /// Maximum number of users allowed for this tenant
        /// </summary>
        public int MaxUsers { get; set; } = 5;

        /// <summary>
        /// Maximum number of printers allowed for this tenant
        /// </summary>
        public int MaxPrinters { get; set; } = 10;

        /// <summary>
        /// Maximum number of reports allowed for this tenant
        /// </summary>
        public int MaxReports { get; set; } = 10;

        /// <summary>
        /// Maximum storage space in MB for this tenant
        /// </summary>
        public long StorageQuotaMB { get; set; } = 1024;

        /// <summary>
        /// Whether the tenant has access to advanced features
        /// </summary>
        public bool HasAdvancedFeatures { get; set; }

        /// <summary>
        /// Whether the tenant has access to premium support
        /// </summary>
        public bool HasPremiumSupport { get; set; }

        /// <summary>
        /// Custom settings for the tenant (JSON format)
        /// </summary>
        public string? Settings { get; set; }

        /// <summary>
        /// Navigation property for users in this tenant
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Navigation property for printers in this tenant
        /// </summary>
        public virtual ICollection<Printer> Printers { get; set; } = new List<Printer>();

        /// <summary>
        /// Navigation property for scheduled reports in this tenant
        /// </summary>
        public virtual ICollection<ScheduledReport> ScheduledReports { get; set; } = new List<ScheduledReport>();
    }
}
