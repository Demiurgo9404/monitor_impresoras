using System.ComponentModel;

namespace MonitorImpresoras.Domain.Enums
{
    /// <summary>
    /// Estados de las impresoras
    /// </summary>
    public enum PrinterStatus
    {
        /// <summary>
        /// Estado desconocido
        /// </summary>
        [Description("Estado desconocido")]
        Unknown,

        /// <summary>
        /// Impresora en línea y funcionando
        /// </summary>
        [Description("En línea")]
        Online,

        /// <summary>
        /// Impresora fuera de línea
        /// </summary>
        [Description("Fuera de línea")]
        Offline,

        /// <summary>
        /// Impresora con error
        /// </summary>
        [Description("Con error")]
        Error,

        /// <summary>
        /// Impresora imprimiendo
        /// </summary>
        [Description("Imprimiendo")]
        Printing,

        /// <summary>
        /// Impresora en pausa
        /// </summary>
        [Description("En pausa")]
        Paused,

        /// <summary>
        /// Impresora ocupada
        /// </summary>
        [Description("Ocupada")]
        Busy,

        /// <summary>
        /// Impresora en modo de ahorro de energía
        /// </summary>
        [Description("Modo ahorro de energía")]
        PowerSave,

        /// <summary>
        /// Impresora en mantenimiento
        /// </summary>
        [Description("En mantenimiento")]
        Maintenance,

        /// <summary>
        /// Impresora sin conexión de red
        /// </summary>
        [Description("Sin conexión")]
        Disconnected
    }

    /// <summary>
    /// Estado de una impresora obtenido vía SNMP
    /// </summary>
    public class PrinterStatusInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int? TonerLevel { get; set; }
        public int? PaperLevel { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
