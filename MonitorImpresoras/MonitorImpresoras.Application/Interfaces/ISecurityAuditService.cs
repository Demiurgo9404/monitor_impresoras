using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de auditoría de seguridad
    /// </summary>
    public interface ISecurityAuditService
    {
        /// <summary>
        /// Realiza auditoría completa de seguridad del sistema
        /// </summary>
        Task<SecurityAuditReport> PerformSecurityAuditAsync();

        /// <summary>
        /// Verifica cumplimiento de estándares de seguridad
        /// </summary>
        Task<SecurityComplianceReport> CheckSecurityComplianceAsync();
    }
}
