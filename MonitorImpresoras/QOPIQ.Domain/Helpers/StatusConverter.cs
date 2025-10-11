using QOPIQ.Domain.Enums;
using System;

// NOTA: Este convertidor ahora usa TransactionStatus unificado

namespace QOPIQ.Domain.Helpers
{
    /// <summary>
    /// Helper class for converting between different status enums
    /// </summary>
    public static class StatusConverter
    {
        /// <summary>
        /// Converts a TransactionStatus to a display-friendly string
        /// </summary>
        public static string ToDisplayString(this TransactionStatus status)
        {
            return status switch
            {
                TransactionStatus.Draft => "Borrador",
                TransactionStatus.Pending => "Pendiente",
                TransactionStatus.Completed => "Completado",
                TransactionStatus.Failed => "Fallido",
                TransactionStatus.Refunded => "Reembolsado",
                TransactionStatus.PartiallyRefunded => "Reembolso Parcial",
                TransactionStatus.Voided => "Anulado",
                TransactionStatus.Overdue => "Vencido",
                _ => "Desconocido"
            };
        }

        /// <summary>
        /// Determines if a status represents a completed transaction
        /// </summary>
        public static bool IsCompleted(this TransactionStatus status)
        {
            return status == TransactionStatus.Completed || 
                   status == TransactionStatus.Refunded ||
                   status == TransactionStatus.PartiallyRefunded;
        }

        /// <summary>
        /// Determines if a status represents a pending transaction
        /// </summary>
        public static bool IsPending(this TransactionStatus status)
        {
            return status == TransactionStatus.Pending || 
                   status == TransactionStatus.Draft;
        }
    }
}
