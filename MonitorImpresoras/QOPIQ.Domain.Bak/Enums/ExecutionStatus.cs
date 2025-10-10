namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Estados de ejecución de reportes
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// Pendiente
        /// </summary>
        Pending,

        /// <summary>
        /// En ejecución
        /// </summary>
        Running,

        /// <summary>
        /// Completado exitosamente
        /// </summary>
        Completed,

        /// <summary>
        /// Fallido
        /// </summary>
        Failed,

        /// <summary>
        /// Cancelado
        /// </summary>
        Cancelled
    }
}
