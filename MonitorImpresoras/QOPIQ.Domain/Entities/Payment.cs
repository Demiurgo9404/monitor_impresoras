using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

// NOTA: Esta entidad ahora usa TransactionStatus en lugar de PaymentStatus

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a payment in the system
    /// </summary>
    public class Payment : BaseEntity
    {
        /// <summary>
        /// ID of the invoice this payment is for
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// The invoice this payment is for
        /// </summary>
        public virtual Invoice? Invoice { get; set; }

        /// <summary>
        /// Amount of the payment
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency of the payment (e.g., USD, EUR, MXN)
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Date and time when the payment was made
        /// </summary>
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Payment method used (e.g., Credit Card, Bank Transfer, PayPal)
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Transaction ID from the payment processor
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Status of the payment
        /// </summary>
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        /// <summary>
        /// Additional notes or details about the payment
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID of the user who processed the payment (if applicable)
        /// </summary>
        public Guid? ProcessedBy { get; set; }

        /// <summary>
        /// Date and time when the payment was processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// ID of the tenant this payment belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this payment belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }
    }
}
