using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents an invoice in the system
    /// </summary>
    public class Invoice : BaseEntity
    {
        /// <summary>
        /// Invoice number (e.g., INV-2023-001)
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID of the subscription this invoice is for
        /// </summary>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// The subscription this invoice is for
        /// </summary>
        public virtual Subscription? Subscription { get; set; }

        /// <summary>
        /// ID of the tenant this invoice belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this invoice belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// Date when the invoice was issued
        /// </summary>
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Due date for the invoice payment
        /// </summary>
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

        /// <summary>
        /// Date when the invoice was paid (if applicable)
        /// </summary>
        public DateTime? PaidDate { get; set; }

        /// <summary>
        /// Status of the invoice
        /// </summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        /// <summary>
        /// Subtotal amount (before taxes and discounts)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Total amount due (including taxes and discounts)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Currency code (e.g., USD, EUR)
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Payment method used (if applicable)
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Transaction ID from the payment processor (if applicable)
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Notes or additional information about the invoice
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Invoice line items
        /// </summary>
        public virtual ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    }

    /// <summary>
    /// Represents a line item on an invoice
    /// </summary>
    public class InvoiceLineItem : BaseEntity
    {
        /// <summary>
        /// ID of the invoice this line item belongs to
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// The invoice this line item belongs to
        /// </summary>
        public virtual Invoice? Invoice { get; set; }

        /// <summary>
        /// Description of the line item
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Quantity of the item
        /// </summary>
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// Unit price of the item
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Tax rate for this line item (e.g., 0.21 for 21%)
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Discount amount for this line item
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Total amount for this line item (including tax and discount)
        /// </summary>
        public decimal TotalAmount => (Quantity * UnitPrice - DiscountAmount) * (1 + TaxRate);
    }
}
