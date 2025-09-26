namespace MonitorImpresoras.Domain.Enums
{
    /// <summary>
    /// Estados de alertas
    /// </summary>
    public enum AlertStatus
    {
        /// <summary>
        /// Abierta
        /// </summary>
        Open,

        /// <summary>
        /// Reconocida
        /// </summary>
        Acknowledged,

        /// <summary>
        /// En progreso
        /// </summary>
        InProgress,

        /// <summary>
        /// Resuelta
        /// </summary>
        Resolved,

        /// <summary>
        /// Cerrada
        /// </summary>
        Closed,

        /// <summary>
        /// Descartada
        /// </summary>
        Dismissed
    }
}
