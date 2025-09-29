using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de escalamiento de notificaciones
    /// </summary>
    public interface INotificationEscalationService
    {
        /// <summary>
        /// Inicia el seguimiento de escalamiento para una notificación crítica
        /// </summary>
        Task StartEscalationTrackingAsync(
            string notificationId,
            List<string> originalRecipients,
            NotificationSeverity severity,
            Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Marca una notificación como reconocida
        /// </summary>
        Task<bool> AcknowledgeNotificationAsync(string notificationId, string userId, string? comments = null);

        /// <summary>
        /// Obtiene el historial de escalamiento de una notificación
        /// </summary>
        Task<IEnumerable<NotificationEscalationHistory>> GetEscalationHistoryAsync(string notificationId);

        /// <summary>
        /// Obtiene estadísticas de escalamiento
        /// </summary>
        Task<EscalationStatisticsDto> GetEscalationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}
