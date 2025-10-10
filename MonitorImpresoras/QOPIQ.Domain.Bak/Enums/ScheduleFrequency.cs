using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Frecuencia de programación de tareas
    /// </summary>
    public enum ScheduleFrequency
    {
        /// <summary>
        /// Una vez al día
        /// </summary>
        [Description("Una vez al día")]
        OnceDaily = 1,

        /// <summary>
        /// Dos veces al día
        /// </summary>
        [Description("Dos veces al día")]
        TwiceDaily = 2,

        /// <summary>
        /// Tres veces al día
        /// </summary>
        [Description("Tres veces al día")]
        ThreeTimesDaily = 3,

        /// <summary>
        /// Cada hora
        /// </summary>
        [Description("Cada hora")]
        Hourly = 24,

        /// <summary>
        /// Cada 30 minutos
        /// </summary>
        [Description("Cada 30 minutos")]
        Every30Minutes = 48,

        /// <summary>
        /// Cada 15 minutos
        /// </summary>
        [Description("Cada 15 minutos")]
        Every15Minutes = 96
    }
}
