using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de notificaciones por email usando SMTP
    /// </summary>
    public class EmailNotificationService : BaseNotificationService
    {
        private readonly SmtpClient _smtpClient;

        public EmailNotificationService(
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger,
            IExtendedAuditService auditService) : base(configuration, logger, auditService)
        {
            var smtpConfig = _configuration.GetSection("Email");

            _smtpClient = new SmtpClient
            {
                Host = smtpConfig["SmtpHost"] ?? "smtp.gmail.com",
                Port = int.Parse(smtpConfig["SmtpPort"] ?? "587"),
                EnableSsl = bool.Parse(smtpConfig["UseSsl"] ?? "true"),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 30000 // 30 segundos
            };

            // Configurar credenciales si están disponibles
            var username = smtpConfig["Username"];
            var password = smtpConfig["Password"];

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                _smtpClient.Credentials = new NetworkCredential(username, password);
            }
        }

        protected override async Task<NotificationResponseDto> SendByChannelAsync(NotificationRequestDto request, NotificationChannel channel)
        {
            if (channel != NotificationChannel.Email)
            {
                throw new NotSupportedException($"Canal {channel} no soportado por {nameof(EmailNotificationService)}");
            }

            var fromAddress = _configuration["Email:FromAddress"] ?? _configuration["Email:Username"] ?? "noreply@monitorimpresoras.com";
            var fromName = _configuration["Email:FromName"] ?? "Sistema de Monitor de Impresoras";

            var response = new NotificationResponseDto
            {
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.Email,
                RecipientsCount = request.Recipients.Count
            };

            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromAddress, fromName),
                    Subject = FormatEmailSubject(request),
                    Body = FormatEmailBody(request),
                    IsBodyHtml = true,
                    Priority = GetEmailPriority(request.Severity)
                };

                // Agregar destinatarios
                foreach (var recipient in request.Recipients)
                {
                    mailMessage.To.Add(recipient);
                }

                // Agregar metadata como headers personalizados
                if (request.Metadata != null)
                {
                    foreach (var metadata in request.Metadata)
                    {
                        mailMessage.Headers.Add($"X-MonitorImpresoras-{metadata.Key}", metadata.Value.ToString());
                    }
                }

                // Agregar header de severidad
                mailMessage.Headers.Add("X-MonitorImpresoras-Severity", request.Severity.ToString());

                // Enviar email
                await _smtpClient.SendMailAsync(mailMessage);

                response.Success = true;
                _logger.LogInformation("Email notification sent successfully to {RecipientsCount} recipients", request.Recipients.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to send email notification");
                throw;
            }

            return response;
        }

        /// <summary>
        /// Formatea el cuerpo del email con HTML
        /// </summary>
        private string FormatEmailBody(NotificationRequestDto request)
        {
            var sb = new StringBuilder();

            // Header con severidad
            var severityColor = request.Severity switch
            {
                NotificationSeverity.Critical => "#dc3545",
                NotificationSeverity.Warning => "#ffc107",
                NotificationSeverity.Info => "#17a2b8",
                _ => "#6c757d"
            };

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'>");
            sb.AppendLine("<title>").Append(request.Title).AppendLine("</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            sb.AppendLine(".header { text-align: center; margin-bottom: 30px; }");
            sb.AppendLine($".severity {{ background-color: {severityColor}; color: white; padding: 10px; border-radius: 5px; display: inline-block; }}");
            sb.AppendLine(".content { line-height: 1.6; margin: 20px 0; }");
            sb.AppendLine(".footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; font-size: 12px; color: #6c757d; }");
            sb.AppendLine(".metadata { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; }");
            sb.AppendLine(".metadata h4 { margin-top: 0; color: #495057; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>").Append(request.Title).AppendLine("</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='content'>");
            sb.AppendLine("<p>").Append(request.Message.Replace("\n", "<br>")).AppendLine("</p>");
            sb.AppendLine("</div>");

            // Agregar metadata si existe
            if (request.Metadata != null && request.Metadata.Count > 0)
            {
                sb.AppendLine("<div class='metadata'>");
                sb.AppendLine("<h4>📊 Información Adicional</h4>");
                sb.AppendLine("<ul>");
                foreach (var metadata in request.Metadata)
                {
                    sb.AppendLine($"<li><strong>{metadata.Key}:</strong> {metadata.Value}</li>");
                }
                sb.AppendLine("</ul>");
                sb.AppendLine("</div>");
            }

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p><strong>Sistema de Monitor de Impresoras</strong></p>");
            sb.AppendLine($"<p>Enviado: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}</p>");
            if (request.RequireAcknowledgment)
            {
                sb.AppendLine("<p><em>Esta alerta requiere reconocimiento.</em></p>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// Obtiene la prioridad del email según la severidad
        /// </summary>
        private MailPriority GetEmailPriority(NotificationSeverity severity)
        {
            return severity switch
            {
                NotificationSeverity.Critical => MailPriority.High,
                NotificationSeverity.Warning => MailPriority.Normal,
                NotificationSeverity.Info => MailPriority.Low,
                _ => MailPriority.Normal
            };
        }

        /// <summary>
        /// Prueba la configuración SMTP
        /// </summary>
        public async Task<bool> TestSmtpConfigurationAsync()
        {
            try
            {
                // Enviar email de prueba
                var testResponse = await SendInfoAsync(
                    "🧪 Prueba de Configuración SMTP",
                    $"Prueba de configuración SMTP realizada el {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}"
                );

                return testResponse.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SMTP configuration");
                return false;
            }
        }

        /// <summary>
        /// Envía reporte diario consolidado
        /// </summary>
        public async Task<NotificationResponseDto> SendDailyReportAsync(
            string reportTitle,
            string reportContent,
            Dictionary<string, object>? metadata = null)
        {
            var request = new NotificationRequestDto
            {
                Title = $"📊 Reporte Diario - {DateTime.UtcNow:dd/MM/yyyy}",
                Message = reportContent,
                Severity = NotificationSeverity.Info,
                Recipients = GetDefaultRecipients(),
                Channels = new List<NotificationChannel> { NotificationChannel.Email },
                RequireAcknowledgment = false,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            return (await SendNotificationAsync(request)).FirstOrDefault() ?? new NotificationResponseDto();
        }
    }
}
