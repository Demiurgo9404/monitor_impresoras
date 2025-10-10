using QOPIQ.Domain.Enums;
using System;

namespace QOPIQ.Domain.Helpers
{
    /// <summary>
    /// Helper class for converting between different status enums
    /// </summary>
    public static class StatusConverter
    {
        /// <summary>
        /// Converts a PaymentStatus to an InvoiceStatus
        /// </summary>
        public static InvoiceStatus ToInvoiceStatus(this PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => InvoiceStatus.Pending,
                PaymentStatus.Completed => InvoiceStatus.Paid,
                PaymentStatus.Failed => InvoiceStatus.Overdue,
                PaymentStatus.Refunded => InvoiceStatus.Refunded,
                PaymentStatus.PartiallyRefunded => InvoiceStatus.PartiallyPaid,
                PaymentStatus.Voided => InvoiceStatus.Voided,
                _ => InvoiceStatus.Draft,
            };
        }

        /// <summary>
        /// Converts an InvoiceStatus to a PaymentStatus
        /// </summary>
        public static PaymentStatus ToPaymentStatus(this InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => PaymentStatus.Pending,
                InvoiceStatus.Sent => PaymentStatus.Pending,
                InvoiceStatus.Pending => PaymentStatus.Pending,
                InvoiceStatus.PartiallyPaid => PaymentStatus.PartiallyRefunded,
                InvoiceStatus.Paid => PaymentStatus.Completed,
                InvoiceStatus.Overdue => PaymentStatus.Failed,
                InvoiceStatus.Voided => PaymentStatus.Voided,
                InvoiceStatus.Refunded => PaymentStatus.Refunded,
                _ => PaymentStatus.Unknown,
            };
        }
    }
}
