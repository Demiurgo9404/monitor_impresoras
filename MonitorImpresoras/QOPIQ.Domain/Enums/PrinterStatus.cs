using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Estado de una impresora en el sistema
    /// </summary>
    public enum PrinterStatus
    {
        /// <summary>
        /// Estado desconocido
        /// </summary>
        [Description("Desconocido")]
        Unknown = 0,

        /// <summary>
        /// Impresora en línea y funcionando correctamente
        /// </summary>
        [Description("En línea")]
        Online = 1,

        /// <summary>
        /// Impresora fuera de línea
        /// </summary>
        [Description("Fuera de línea")]
        Offline = 2,

        /// <summary>
        /// Impresora con error
        /// </summary>
        [Description("Con error")]
        Error = 3,

        /// <summary>
        /// Impresora con advertencia
        /// </summary>
        [Description("Advertencia")]
        Warning = 4,

        /// <summary>
        /// Impresora en mantenimiento
        /// </summary>
        [Description("En mantenimiento")]
        Maintenance = 5
    }
}

