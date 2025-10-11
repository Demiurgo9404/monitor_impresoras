namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Unified status for all financial transactions
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Transaction is in draft state
        /// </summary>
        Draft = 0,
        
        /// <summary>
        /// Transaction is pending processing
        /// </summary>
        Pending = 1,
        
        /// <summary>
        /// Transaction has been processed successfully
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Transaction has failed
        /// </summary>
        Failed = 3,
        
        /// <summary>
        /// Transaction has been refunded
        /// </summary>
        Refunded = 4,
        
        /// <summary>
        /// Transaction has been partially refunded
        /// </summary>
        PartiallyRefunded = 5,
        
        /// <summary>
        /// Transaction has been voided
        /// </summary>
        Voided = 6,
        
        /// <summary>
        /// Transaction is overdue
        /// </summary>
        Overdue = 7,
        
        /// <summary>
        /// Unknown status
        /// </summary>
        Unknown = 99
    }
}
