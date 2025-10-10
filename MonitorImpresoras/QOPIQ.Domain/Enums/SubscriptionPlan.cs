using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Planes de suscripción disponibles
    /// </summary>
    public enum SubscriptionPlan
    {
        /// <summary>
        /// Plan gratuito con funcionalidades básicas
        /// </summary>
        [Description("Gratuito")]
        Free = 0,

        /// <summary>
        /// Plan básico para pequeñas empresas
        /// </summary>
        [Description("Básico")]
        Basic = 1,

        /// <summary>
        /// Plan profesional con características avanzadas
        /// </summary>
        [Description("Profesional")]
        Professional = 2,

        /// <summary>
        /// Plan empresarial con soporte prioritario
        /// </summary>
        [Description("Empresarial")]
        Enterprise = 3
    }
}
