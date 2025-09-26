namespace MonitorImpresoras.Domain.Enums
{
    /// <summary>
    /// Tipos de alertas
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// Notificación del sistema
        /// </summary>
        SystemNotification,

        /// <summary>
        /// Error de impresora
        /// </summary>
        PrinterError,

        /// <summary>
        /// Advertencia de consumible
        /// </summary>
        ConsumableWarning,

        /// <summary>
        /// Impresora desconectada
        /// </summary>
        PrinterOffline,

        /// <summary>
        /// Consumible bajo
        /// </summary>
        LowConsumable,

        /// <summary>
        /// Consumible crítico
        /// </summary>
        CriticalConsumable,

        /// <summary>
        /// Uso inusual
        /// </summary>
        UnusualUsage,

        /// <summary>
        /// Costo elevado
        /// </summary>
        HighCost,

        /// <summary>
        /// Mantenimiento requerido
        /// </summary>
        Maintenance,

        /// <summary>
        /// Alerta personalizada
        /// </summary>
        Custom
    }
}
