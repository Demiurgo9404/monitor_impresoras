using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de seguridad de API
    /// </summary>
    public interface IApiSecurityService
    {
        /// <summary>
        /// Ejecuta configuración completa de seguridad de API
        /// </summary>
        Task<ApiSecurityResult> ConfigureApiSecurityAsync();

        /// <summary>
        /// Valida entrada contra ataques comunes
        /// </summary>
        bool ValidateInput(string input, InputValidationType validationType);

        /// <summary>
        /// Registra evento de seguridad para auditoría
        /// </summary>
        void LogSecurityEvent(string eventType, string description, string userId, string ipAddress, bool isSuspicious = false);

        /// <summary>
        /// Obtiene recomendaciones adicionales de seguridad para API
        /// </summary>
        Task<IEnumerable<ApiSecurityRecommendation>> GetSecurityRecommendationsAsync();
    }
}
