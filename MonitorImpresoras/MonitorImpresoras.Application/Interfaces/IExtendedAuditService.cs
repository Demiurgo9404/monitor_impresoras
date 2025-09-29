using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de auditoría extendida
    /// </summary>
    public interface IExtendedAuditService
    {
        /// <summary>
        /// Registra un evento en el sistema de auditoría
        /// </summary>
        Task LogEventAsync(SystemEvent systemEvent);

        /// <summary>
        /// Obtiene eventos del sistema con filtros
        /// </summary>
        Task<IEnumerable<SystemEventDto>> GetSystemEventsAsync(
            string? eventType = null,
            string? category = null,
            string? severity = null,
            string? userId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 50);

        /// <summary>
        /// Obtiene estadísticas de eventos del sistema
        /// </summary>
        Task<SystemEventStatisticsDto> GetSystemEventStatisticsAsync(
            DateTime? dateFrom = null,
            DateTime? dateTo = null);

        /// <summary>
        /// Limpia eventos antiguos
        /// </summary>
        Task<int> CleanupOldEventsAsync(int retentionDays = 90);

        /// <summary>
        /// Registra evento relacionado con reportes
        /// </summary>
        Task LogReportEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string? userId = null, bool isSuccess = true,
            string? errorMessage = null, long? executionTimeMs = null);

        /// <summary>
        /// Registra evento relacionado con emails
        /// </summary>
        Task LogEmailEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string? userId = null, bool isSuccess = true,
            string? errorMessage = null);

        /// <summary>
        /// Registra evento relacionado con seguridad
        /// </summary>
        Task LogSecurityEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string? userId = null, bool isSuccess = true,
            string? errorMessage = null, string severity = "Info");

        /// <summary>
        /// Registra evento general del sistema
        /// </summary>
        Task LogSystemEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string severity = "Info", bool isSuccess = true);
    }
}
