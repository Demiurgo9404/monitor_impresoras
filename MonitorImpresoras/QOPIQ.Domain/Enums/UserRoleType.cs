using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// User role types in the system
    /// </summary>
    public enum UserRoleType
    {
        /// <summary>
        /// System administrator with full access
        /// </summary>
        [Description("System Administrator")]
        SuperAdmin = 0,

        /// <summary>
        /// Tenant administrator with full access to tenant resources
        /// </summary>
        [Description("Administrator")]
        Admin = 1,

        /// <summary>
        /// Regular user with basic access
        /// </summary>
        [Description("User")]
        User = 2,

        /// <summary>
        /// Read-only user with view access
        /// </summary>
        [Description("Viewer")]
        Viewer = 3,

        /// <summary>
        /// Printer agent service account
        /// </summary>
        [Description("Service Account")]
        ServiceAccount = 4,

        /// <summary>
        /// Support staff with limited admin access
        /// </summary>
        [Description("Support Staff")]
        Support = 5
    }
}
