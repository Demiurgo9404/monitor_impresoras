using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Planes de suscripción disponibles
    /// </summary>
    public enum SubscriptionPlan
    {
        Free = 0,       // 5 impresoras, $0/mes
        Basic = 1,      // 25 impresoras, $29/mes  
        Pro = 2,        // 100 impresoras, $99/mes
        Enterprise = 3  // Ilimitado, $299/mes
    }

    /// <summary>
    /// Estados de suscripción
    /// </summary>
    public enum SubscriptionStatus
    {
        Active = 0,
        Expired = 1,
        Cancelled = 2,
        PendingPayment = 3,
        Suspended = 4
    }

    /// <summary>
    /// Estados de factura
    /// </summary>
    public enum InvoiceStatus
    {
        Pending = 0,
        Paid = 1,
        Overdue = 2,
        Cancelled = 3,
        Failed = 4
    }

    /// <summary>
    /// Entidad de suscripción del usuario
    /// </summary>
    public class Subscription : BaseEntity
    {
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        // Agregar PlanId que faltaba
        public Guid PlanId { get; set; }

        [Required]
        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;

        [Required]
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MonthlyPrice { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        // Stripe Integration
        [MaxLength(100)]
        public string? StripeCustomerId { get; set; }

        [MaxLength(100)]
        public string? StripeSubscriptionId { get; set; }

        [MaxLength(100)]
        public string? StripePriceId { get; set; }

        // Límites del plan
        public int MaxPrinters { get; set; }
        public int MaxUsers { get; set; }
        public int MaxReportsPerMonth { get; set; }
        public bool HasAdvancedAnalytics { get; set; }
        public bool HasApiAccess { get; set; }

        [MaxLength(50)]
        public string SupportLevel { get; set; } = "Community";

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        // Computed properties
        [NotMapped]
        public bool IsActive => Status == SubscriptionStatus.Active && EndDate > DateTime.UtcNow;

        [NotMapped]
        public int DaysRemaining => (EndDate - DateTime.UtcNow).Days;

        [NotMapped]
        public bool IsExpiringSoon => DaysRemaining <= 7 && DaysRemaining > 0;

        /// <summary>
        /// Obtiene los límites del plan según el tipo
        /// </summary>
        public void SetPlanLimits()
        {
            switch (Plan)
            {
                case SubscriptionPlan.Free:
                    MaxPrinters = 5;
                    MaxUsers = 2;
                    MaxReportsPerMonth = 10;
                    HasAdvancedAnalytics = false;
                    HasApiAccess = false;
                    SupportLevel = "Community";
                    MonthlyPrice = 0;
                    break;

                case SubscriptionPlan.Basic:
                    MaxPrinters = 25;
                    MaxUsers = 10;
                    MaxReportsPerMonth = 100;
                    HasAdvancedAnalytics = false;
                    HasApiAccess = true;
                    SupportLevel = "Email";
                    MonthlyPrice = 29;
                    break;

                case SubscriptionPlan.Pro:
                    MaxPrinters = 100;
                    MaxUsers = 50;
                    MaxReportsPerMonth = 500;
                    HasAdvancedAnalytics = true;
                    HasApiAccess = true;
                    SupportLevel = "Priority";
                    MonthlyPrice = 99;
                    break;

                case SubscriptionPlan.Enterprise:
                    MaxPrinters = int.MaxValue;
                    MaxUsers = int.MaxValue;
                    MaxReportsPerMonth = int.MaxValue;
                    HasAdvancedAnalytics = true;
                    HasApiAccess = true;
                    SupportLevel = "Dedicated";
                    MonthlyPrice = 299;
                    break;
            }
        }
    }

    /// <summary>
    /// Entidad de factura
    /// </summary>
    public class Invoice : BaseEntity
    {
        [Required]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? PaidAt { get; set; }

        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Stripe Integration
        [MaxLength(100)]
        public string? StripeInvoiceId { get; set; }

        [MaxLength(100)]
        public string? StripePaymentIntentId { get; set; }

        // Navigation properties
        public virtual Subscription Subscription { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public bool IsOverdue => Status == InvoiceStatus.Pending && DueDate < DateTime.UtcNow;

        [NotMapped]
        public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;
    }
}
