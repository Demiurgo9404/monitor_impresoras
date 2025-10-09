using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Estados de suscripción
    /// </summary>
    public enum SubscriptionStatus
    {
        /// <summary>
        /// Estado desconocido
        /// </summary>
        [Description("Estado desconocido")]
        Unknown = 0,

        /// <summary>
        /// Suscripción activa
        /// </summary>
        [Description("Activa")]
        Active = 1,

        /// <summary>
        /// Suscripción inactiva
        /// </summary>
        [Description("Inactiva")]
        Inactive = 2,

        /// <summary>
        /// Suscripción suspendida
        /// </summary>
        [Description("Suspendida")]
        Suspended = 3,

        /// <summary>
        /// Suscripción cancelada
        /// </summary>
        [Description("Cancelada")]
        Cancelled = 4,

        /// <summary>
        /// Suscripción en período de prueba
        /// </summary>
        [Description("En período de prueba")]
        Trial = 5,

        /// <summary>
        /// Suscripción expirada
        /// </summary>
        [Description("Expirada")]
        Expired = 6,

        /// <summary>
        /// Suscripción pendiente de pago
        /// </summary>
        [Description("Pendiente de pago")]
        PendingPayment = 7
    }
}

