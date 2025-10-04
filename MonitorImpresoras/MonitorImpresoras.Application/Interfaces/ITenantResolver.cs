namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Resuelve el tenant actual desde el contexto de la petición HTTP
    /// </summary>
    public interface ITenantResolver
    {
        /// <summary>
        /// Obtiene el ID del tenant actual
        /// </summary>
        string? GetCurrentTenantId();

        /// <summary>
        /// Verifica si el tenant actual es válido
        /// </summary>
        Task<bool> IsValidTenantAsync(string tenantId);

        /// <summary>
        /// Obtiene información completa del tenant actual
        /// </summary>
        Task<TenantInfo?> GetCurrentTenantAsync();
    }

    /// <summary>
    /// Información básica del tenant
    /// </summary>
    public class TenantInfo
    {
        public string TenantId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
