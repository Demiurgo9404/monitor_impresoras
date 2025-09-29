namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de envío de emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un email con adjunto de reporte
        /// </summary>
        Task SendReportEmailAsync(
            IEnumerable<string> recipients,
            string subject,
            string body,
            byte[] attachment,
            string attachmentFileName,
            string contentType);

        /// <summary>
        /// Envía un email simple sin adjuntos
        /// </summary>
        Task SendSimpleEmailAsync(string recipient, string subject, string body);
    }
}
