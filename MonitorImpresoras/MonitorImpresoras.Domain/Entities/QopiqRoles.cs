namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Roles del sistema QOPIQ
    /// </summary>
    public static class QopiqRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string CompanyAdmin = "CompanyAdmin";
        public const string ProjectManager = "ProjectManager";
        public const string User = "User";
        public const string Viewer = "Viewer";
        
        /// <summary>
        /// Obtiene todos los roles disponibles
        /// </summary>
        public static string[] GetAllRoles()
        {
            return new[] { SuperAdmin, CompanyAdmin, ProjectManager, User, Viewer };
        }
        
        /// <summary>
        /// Verifica si un rol es válido
        /// </summary>
        public static bool IsValidRole(string role)
        {
            return GetAllRoles().Contains(role);
        }
    }
}
