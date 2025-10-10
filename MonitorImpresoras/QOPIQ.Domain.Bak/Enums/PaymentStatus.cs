using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Represents the status of a payment
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Payment status is unknown
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        /// <summary>
        /// Payment is pending
        /// </summary>
        [Description("Pending")]
        Pending = 1,

        /// <summary>
        /// Payment has been completed successfully
        /// </summary>
        [Description("Completed")]
        Completed = 2,

        /// <summary>
        /// Payment has failed
        /// </summary>
        [Description("Failed")]
        Failed = 3,

        /// <summary>
        /// Payment has been refunded
        /// </summary>
        [Description("Refunded")]
        Refunded = 4,

        /// <summary>
        /// Payment is in dispute
        /// </summary>
        [Description("Disputed")]
        Disputed = 5,

        /// <summary>
        /// Payment has been canceled
        /// </summary>
        [Description("Canceled")]
        Canceled = 6
    }
}
