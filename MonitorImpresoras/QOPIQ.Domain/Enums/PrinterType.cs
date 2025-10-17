using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Tipos de impresoras soportadas por el sistema
    /// </summary>
    public enum PrinterType
    {
        /// <summary>
        /// Tipo de impresora desconocida
        /// </summary>
        [Description("Desconocido")]
        Unknown = 0,

        /// <summary>
        /// Impresora láser
        /// </summary>
        [Description("Láser")]
        Laser = 1,

        /// <summary>
        /// Impresora de inyección de tinta
        /// </summary>
        [Description("Inyección de tinta")]
        Inkjet = 2,

        /// <summary>
        /// Impresora de matriz de puntos
        /// </summary>
        [Description("Matriz de puntos")]
        DotMatrix = 3,

        /// <summary>
        /// Impresora térmica
        /// </summary>
        [Description("Térmica")]
        Thermal = 4,

        /// <summary>
        /// Impresora LED
        /// </summary>
        [Description("LED")]
        Led = 5,

        /// <summary>
        /// Impresora multifunción
        /// </summary>
        [Description("Multifunción")]
        Multifunction = 6
    }
}
