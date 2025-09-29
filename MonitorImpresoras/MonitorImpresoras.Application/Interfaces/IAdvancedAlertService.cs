using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de alertas avanzadas multi-canal
    /// </summary>
    public interface IAdvancedAlertService
    {
        /// <summary>
        /// Procesa evento y determina si requiere alerta
        /// </summary>
        Task<AlertResult> ProcessAlertEventAsync(AlertEvent alertEvent);

        /// <summary>
        /// Crea alerta dinámica basada en métricas de impresoras
        /// </summary>
        Task<DynamicAlert> CreateDynamicPrinterAlertAsync(PrinterMetrics metrics);

        /// <summary>
        /// Configura reglas de alerta dinámicas
        /// </summary>
        Task<AlertConfigurationResult> ConfigureDynamicAlertRulesAsync();

        /// <summary>
        /// Envía notificación de prueba a todos los canales
        /// </summary>
        Task<NotificationTestResult> SendTestNotificationAsync();

        /// <summary>
        /// Obtiene historial de alertas para análisis
        /// </summary>
        Task<AlertHistoryReport> GetAlertHistoryAsync(DateTime fromDate, DateTime toDate, AlertSeverity? severity = null);
    }
}
