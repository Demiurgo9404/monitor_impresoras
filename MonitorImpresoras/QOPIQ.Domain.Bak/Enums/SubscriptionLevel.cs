using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Represents the subscription levels available for tenants
    /// </summary>
    public enum SubscriptionLevel
    {
        /// <summary>
        /// Free tier with basic features
        /// </summary>
        [Description("Free")]
        Free = 0,

        /// <summary>
        /// Basic tier with standard features
        /// </summary>
        [Description("Basic")]
        Basic = 1,

        /// <summary>
        /// Professional tier with advanced features
        /// </summary>
        [Description("Professional")]
        Professional = 2,

        /// <summary>
        /// Enterprise tier with all features and premium support
        /// </summary>
        [Description("Enterprise")]
        Enterprise = 3
    }
}
