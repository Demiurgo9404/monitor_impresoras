namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio para envío de emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un email simple
        /// </summary>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Envía un email a múltiples destinatarios
        /// </summary>
        Task<bool> SendEmailAsync(string[] to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Envía un email con adjuntos
        /// </summary>
        Task<bool> SendEmailWithAttachmentsAsync(string[] to, string subject, string body, 
            Dictionary<string, byte[]> attachments, bool isHtml = true);

        /// <summary>
        /// Envía un reporte por email
        /// </summary>
        Task<bool> SendReportEmailAsync(string[] to, string reportTitle, byte[] reportData, 
            string fileName, string contentType, string? additionalMessage = null);

        /// <summary>
        /// Envía notificación de reporte programado
        /// </summary>
        Task<bool> SendScheduledReportNotificationAsync(string[] to, string reportName, 
            string projectName, DateTime generatedAt, string downloadUrl);

        /// <summary>
        /// Envía email de bienvenida
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(string to, string userName, string companyName, string tenantId);

        /// <summary>
        /// Envía notificación de alerta crítica
        /// </summary>
        Task<bool> SendCriticalAlertEmailAsync(string[] to, string printerName, string alertMessage, 
            string tenantId, string projectName);

        /// <summary>
        /// Verifica la configuración del servicio de email
        /// </summary>
        Task<bool> TestEmailConfigurationAsync();
    }
}
