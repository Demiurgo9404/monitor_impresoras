using Microsoft.Extensions.Configuration;
using MonitorImpresoras.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de email usando SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Configuración SMTP desde appsettings.json
            _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:Username"] ?? "";
            _smtpPassword = _configuration["Email:Password"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@qopiq.com";
            _fromName = _configuration["Email:FromName"] ?? "QOPIQ Sistema";
            _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            return await SendEmailAsync(new[] { to }, subject, body, isHtml);
        }

        public async Task<bool> SendEmailAsync(string[] to, string subject, string body, bool isHtml = true)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("Email service not configured. Skipping email send.");
                    return false;
                }

                using var client = new SmtpClient(_smtpHost, _smtpPort);
                client.EnableSsl = _enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                foreach (var recipient in to)
                {
                    if (!string.IsNullOrWhiteSpace(recipient))
                    {
                        message.To.Add(recipient);
                    }
                }

                if (message.To.Count == 0)
                {
                    _logger.LogWarning("No valid recipients found for email");
                    return false;
                }

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", to));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipients}", string.Join(", ", to));
                return false;
            }
        }

        public async Task<bool> SendEmailWithAttachmentsAsync(string[] to, string subject, string body, 
            Dictionary<string, byte[]> attachments, bool isHtml = true)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("Email service not configured. Skipping email send.");
                    return false;
                }

                using var client = new SmtpClient(_smtpHost, _smtpPort);
                client.EnableSsl = _enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                foreach (var recipient in to)
                {
                    if (!string.IsNullOrWhiteSpace(recipient))
                    {
                        message.To.Add(recipient);
                    }
                }

                if (message.To.Count == 0)
                {
                    _logger.LogWarning("No valid recipients found for email with attachments");
                    return false;
                }

                // Agregar adjuntos
                foreach (var attachment in attachments)
                {
                    var stream = new MemoryStream(attachment.Value);
                    var mailAttachment = new Attachment(stream, attachment.Key);
                    message.Attachments.Add(mailAttachment);
                }

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email with {AttachmentCount} attachments sent successfully to {Recipients}", 
                    attachments.Count, string.Join(", ", to));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with attachments to {Recipients}", string.Join(", ", to));
                return false;
            }
        }

        public async Task<bool> SendReportEmailAsync(string[] to, string reportTitle, byte[] reportData, 
            string fileName, string contentType, string? additionalMessage = null)
        {
            try
            {
                var subject = $"QOPIQ - {reportTitle}";
                
                var body = new StringBuilder();
                body.AppendLine("<html><body>");
                body.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
                body.AppendLine("<div style='background-color: #2563eb; color: white; padding: 20px; text-align: center;'>");
                body.AppendLine("<h1>QOPIQ - Sistema de Monitoreo de Impresoras</h1>");
                body.AppendLine("</div>");
                body.AppendLine("<div style='padding: 20px; background-color: #f8fafc;'>");
                body.AppendLine($"<h2>📊 {reportTitle}</h2>");
                body.AppendLine("<p>Se ha generado un nuevo reporte de monitoreo de impresoras.</p>");
                
                if (!string.IsNullOrEmpty(additionalMessage))
                {
                    body.AppendLine($"<p>{additionalMessage}</p>");
                }
                
                body.AppendLine("<div style='background-color: white; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
                body.AppendLine("<h3>📋 Detalles del Reporte</h3>");
                body.AppendLine($"<p><strong>Título:</strong> {reportTitle}</p>");
                body.AppendLine($"<p><strong>Archivo:</strong> {fileName}</p>");
                body.AppendLine($"<p><strong>Generado:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</p>");
                body.AppendLine($"<p><strong>Tamaño:</strong> {FormatFileSize(reportData.Length)}</p>");
                body.AppendLine("</div>");
                
                body.AppendLine("<p>El reporte se encuentra adjunto a este correo electrónico.</p>");
                body.AppendLine("<div style='background-color: #e0f2fe; padding: 15px; border-radius: 8px; border-left: 4px solid #0ea5e9;'>");
                body.AppendLine("<p><strong>💡 Nota:</strong> Este es un reporte automático generado por el sistema QOPIQ. ");
                body.AppendLine("Para más información, acceda al panel de control de su tenant.</p>");
                body.AppendLine("</div>");
                body.AppendLine("</div>");
                body.AppendLine("<div style='background-color: #64748b; color: white; padding: 15px; text-align: center; font-size: 12px;'>");
                body.AppendLine("<p>QOPIQ - Sistema de Monitoreo de Impresoras</p>");
                body.AppendLine($"<p>Generado automáticamente el {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</p>");
                body.AppendLine("</div>");
                body.AppendLine("</div>");
                body.AppendLine("</body></html>");

                var attachments = new Dictionary<string, byte[]>
                {
                    [fileName] = reportData
                };

                return await SendEmailWithAttachmentsAsync(to, subject, body.ToString(), attachments, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending report email");
                return false;
            }
        }

        public async Task<bool> SendScheduledReportNotificationAsync(string[] to, string reportName, 
            string projectName, DateTime generatedAt, string downloadUrl)
        {
            try
            {
                var subject = $"QOPIQ - Reporte Programado: {reportName}";
                
                var body = new StringBuilder();
                body.AppendLine("<html><body>");
                body.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
                body.AppendLine("<div style='background-color: #2563eb; color: white; padding: 20px; text-align: center;'>");
                body.AppendLine("<h1>QOPIQ - Reporte Programado</h1>");
                body.AppendLine("</div>");
                body.AppendLine("<div style='padding: 20px; background-color: #f8fafc;'>");
                body.AppendLine($"<h2>⏰ {reportName}</h2>");
                body.AppendLine("<p>Su reporte programado ha sido generado exitosamente.</p>");
                
                body.AppendLine("<div style='background-color: white; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
                body.AppendLine("<h3>📋 Información del Reporte</h3>");
                body.AppendLine($"<p><strong>Nombre:</strong> {reportName}</p>");
                body.AppendLine($"<p><strong>Proyecto:</strong> {projectName}</p>");
                body.AppendLine($"<p><strong>Generado:</strong> {generatedAt:dd/MM/yyyy HH:mm} UTC</p>");
                body.AppendLine("</div>");
                
                body.AppendLine("<div style='text-align: center; margin: 30px 0;'>");
                body.AppendLine($"<a href='{downloadUrl}' style='background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;'>📥 Descargar Reporte</a>");
                body.AppendLine("</div>");
                
                body.AppendLine("<div style='background-color: #f0f9ff; padding: 15px; border-radius: 8px; border-left: 4px solid #0ea5e9;'>");
                body.AppendLine("<p><strong>💡 Recordatorio:</strong> Este reporte estará disponible para descarga durante 30 días. ");
                body.AppendLine("También puede acceder a todos sus reportes desde el panel de control.</p>");
                body.AppendLine("</div>");
                body.AppendLine("</div>");
                body.AppendLine("<div style='background-color: #64748b; color: white; padding: 15px; text-align: center; font-size: 12px;'>");
                body.AppendLine("<p>QOPIQ - Sistema de Monitoreo de Impresoras</p>");
                body.AppendLine($"<p>Notificación automática - {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</p>");
                body.AppendLine("</div>");
                body.AppendLine("</div>");
                body.AppendLine("</body></html>");

                return await SendEmailAsync(to, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending scheduled report notification");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string to, string userName, string companyName, string tenantId)
        {
            try
            {
                var subject = "¡Bienvenido a QOPIQ!";
                
                var body = new StringBuilder();
                body.AppendLine("<html><body>");
                body.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
                body.AppendLine("<div style='background-color: #2563eb; color: white; padding: 20px; text-align: center;'>");
                body.AppendLine("<h1>🎉 ¡Bienvenido a QOPIQ!</h1>");
                body.AppendLine("</div>");
                body.AppendLine("<div style='padding: 20px; background-color: #f8fafc;'>");
                body.AppendLine($"<h2>Hola {userName},</h2>");
                body.AppendLine($"<p>¡Bienvenido al sistema QOPIQ de <strong>{companyName}</strong>!</p>");
                body.AppendLine("<p>Ya puedes comenzar a monitorear tus impresoras y generar reportes detallados.</p>");
                
                body.AppendLine("<div style='background-color: white; padding: 15px; border-radius: 8px; margin: 20px 0;'>");
                body.AppendLine("<h3>🚀 Primeros Pasos</h3>");
                body.AppendLine("<ul>");
                body.AppendLine("<li>Configura tus proyectos de monitoreo</li>");
                body.AppendLine("<li>Agrega las impresoras a monitorear</li>");
                body.AppendLine("<li>Programa reportes automáticos</li>");
                body.AppendLine("<li>Configura alertas personalizadas</li>");
                body.AppendLine("</ul>");
                body.AppendLine("</div>");
                
                body.AppendLine($"<p><strong>ID de Tenant:</strong> {tenantId}</p>");
                body.AppendLine("<p>Si tienes alguna pregunta, no dudes en contactar a nuestro equipo de soporte.</p>");
                body.AppendLine("</div>");
                body.AppendLine("<div style='background-color: #64748b; color: white; padding: 15px; text-align: center; font-size: 12px;'>");
                body.AppendLine("<p>QOPIQ - Sistema de Monitoreo de Impresoras</p>");
                body.AppendLine("<p>Equipo QOPIQ</p>");
                body.AppendLine("</div>");
                body.AppendLine("</div>");
                body.AppendLine("</body></html>");

                return await SendEmailAsync(to, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email");
                return false;
            }
        }

        public async Task<bool> SendCriticalAlertEmailAsync(string[] to, string printerName, string alertMessage, 
            string tenantId, string projectName)
        {
            try
            {
                var subject = $"🚨 ALERTA CRÍTICA - {printerName}";
                
                var body = new StringBuilder();
                body.AppendLine("<html><body>");
                body.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
                body.AppendLine("<div style='background-color: #dc2626; color: white; padding: 20px; text-align: center;'>");
                body.AppendLine("<h1>🚨 ALERTA CRÍTICA</h1>");
                body.AppendLine("</div>");
                body.AppendLine("<div style='padding: 20px; background-color: #fef2f2;'>");
                body.AppendLine($"<h2>Impresora: {printerName}</h2>");
                body.AppendLine($"<p><strong>Proyecto:</strong> {projectName}</p>");
                body.AppendLine($"<p><strong>Mensaje:</strong> {alertMessage}</p>");
                body.AppendLine($"<p><strong>Fecha/Hora:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</p>");
                
                body.AppendLine("<div style='background-color: #fee2e2; padding: 15px; border-radius: 8px; border-left: 4px solid #dc2626; margin: 20px 0;'>");
                body.AppendLine("<p><strong>⚠️ Acción Requerida:</strong> Esta alerta requiere atención inmediata. ");
                body.AppendLine("Por favor, revise el estado de la impresora y tome las medidas necesarias.</p>");
                body.AppendLine("</div>");
                
                body.AppendLine("</div>");
                body.AppendLine("<div style='background-color: #64748b; color: white; padding: 15px; text-align: center; font-size: 12px;'>");
                body.AppendLine("<p>QOPIQ - Sistema de Monitoreo de Impresoras</p>");
                body.AppendLine($"<p>Tenant: {tenantId}</p>");
                body.AppendLine("</div>");
                body.AppendLine("</div>");
                body.AppendLine("</body></html>");

                return await SendEmailAsync(to, subject, body.ToString(), true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending critical alert email");
                return false;
            }
        }

        public async Task<bool> TestEmailConfigurationAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("Email configuration is incomplete");
                    return false;
                }

                using var client = new SmtpClient(_smtpHost, _smtpPort);
                client.EnableSsl = _enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                // Intentar conectar sin enviar email
                await Task.Run(() => client.Send(new MailMessage(_fromEmail, _fromEmail, "Test", "Test")));
                
                _logger.LogInformation("Email configuration test successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email configuration test failed");
                return false;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
