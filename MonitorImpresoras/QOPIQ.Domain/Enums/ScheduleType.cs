using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Represents the type of schedule for a recurring task
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// Run once at a specific time
        /// </summary>
        [Description("Once")]
        Once = 0,

        /// <summary>
        /// Run every hour
        /// </summary>
        [Description("Hourly")]
        Hourly = 1,

        /// <summary>
        /// Run daily at a specific time
        /// </summary>
        [Description("Daily")]
        Daily = 2,

        /// <summary>
        /// Run weekly on specific days
        /// </summary>
        [Description("Weekly")]
        Weekly = 3,

        /// <summary>
        /// Run monthly on a specific day
        /// </summary>
        [Description("Monthly")]
        Monthly = 4,

        /// <summary>
        /// Run at a custom interval (in minutes)
        /// </summary>
        [Description("Custom")]
        Custom = 5
    }
}
