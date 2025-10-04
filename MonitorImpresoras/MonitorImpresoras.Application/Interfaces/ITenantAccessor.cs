namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Proporciona acceso al tenant actual en el contexto de la aplicación
    /// </summary>
    public interface ITenantAccessor
    {
        /// <summary>
        /// ID del tenant actual
        /// </summary>
        string? TenantId { get; }

        /// <summary>
        /// Información del tenant actual
        /// </summary>
        TenantInfo? TenantInfo { get; }

        /// <summary>
        /// Establece el tenant actual (usado por el middleware)
        /// </summary>
        void SetTenant(string tenantId, TenantInfo? tenantInfo = null);

        /// <summary>
        /// Verifica si hay un tenant establecido
        /// </summary>
        bool HasTenant { get; }
    }
}
