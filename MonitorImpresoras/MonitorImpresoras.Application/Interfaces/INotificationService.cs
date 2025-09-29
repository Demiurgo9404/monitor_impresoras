using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de notificaciones
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Envía una notificación crítica
        /// </summary>
        Task<NotificationResponseDto> SendCriticalAsync(string title, string message, List<string>? recipients = null);

        /// <summary>
        /// Envía una notificación de advertencia
        /// </summary>
        Task<NotificationResponseDto> SendWarningAsync(string title, string message, List<string>? recipients = null);

        /// <summary>
        /// Envía una notificación informativa
        /// </summary>
        Task<NotificationResponseDto> SendInfoAsync(string title, string message, List<string>? recipients = null);

        /// <summary>
        /// Envía una notificación personalizada
        /// </summary>
        Task<List<NotificationResponseDto>> SendNotificationAsync(NotificationRequestDto request);

        /// <summary>
        /// Obtiene el historial de notificaciones
        /// </summary>
        Task<IEnumerable<NotificationHistoryDto>> GetNotificationHistoryAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            NotificationSeverity? severity = null,
            NotificationChannel? channel = null,
            string? recipient = null,
            int page = 1,
            int pageSize = 50);

        /// <summary>
        /// Marca una notificación como reconocida
        /// </summary>
        Task<bool> AcknowledgeNotificationAsync(Guid notificationId, string userId);

        /// <summary>
        /// Obtiene estadísticas de notificaciones
        /// </summary>
        Task<NotificationStatisticsDto> GetNotificationStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Prueba la configuración de notificaciones
        /// </summary>
        Task<List<NotificationResponseDto>> TestNotificationConfigurationAsync(List<NotificationChannel> channels);
    }
}
