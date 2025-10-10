using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Represents the status of an invoice
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>
        /// Invoice has been created but not yet paid
        /// </summary>
        [Description("Pending")]
        Pending = 0,

        /// <summary>
        /// Invoice has been paid
        /// </summary>
        [Description("Paid")]
        Paid = 1,

        /// <summary>
        /// Invoice is overdue
        /// </summary>
        [Description("Overdue")]
        Overdue = 2,

        /// <summary>
        /// Invoice has been voided/cancelled
        /// </summary>
        [Description("Void")]
        Void = 3,

        /// <summary>
        /// Payment for the invoice has failed
        /// </summary>
        [Description("Payment Failed")]
        PaymentFailed = 4,

        /// <summary>
        /// Invoice has been refunded
        /// </summary>
        [Description("Refunded")]
        Refunded = 5,

        /// <summary>
        /// Invoice is in dispute
        /// </summary>
        [Description("Disputed")]
        Disputed = 6,

        /// <summary>
        /// Invoice has been partially paid
        /// </summary>
        [Description("Partially Paid")]
        PartiallyPaid = 7
    }
}
