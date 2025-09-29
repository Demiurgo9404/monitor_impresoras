using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de notificaciones para WhatsApp usando Twilio API
    /// </summary>
    public class WhatsAppNotificationService : BaseNotificationService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public WhatsAppNotificationService(
            IConfiguration configuration,
            ILogger<WhatsAppNotificationService> logger,
            IExtendedAuditService auditService) : base(configuration, logger, auditService)
        {
            _accountSid = configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid no configurado");
            _authToken = configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken no configurado");
            _fromPhoneNumber = configuration["Twilio:FromPhoneNumber"] ?? throw new InvalidOperationException("Twilio FromPhoneNumber no configurado");

            TwilioClient.Init(_accountSid, _authToken);
        }

        protected override async Task<NotificationResponseDto> SendByChannelAsync(NotificationRequestDto request, NotificationChannel channel)
        {
            if (channel != NotificationChannel.WhatsApp)
            {
                throw new NotSupportedException($"Canal {channel} no soportado por {nameof(WhatsAppNotificationService)}");
            }

            // Solo enviar mensajes críticos por WhatsApp para evitar spam
            if (request.Severity != NotificationSeverity.Critical)
            {
                _logger.LogInformation("Skipping WhatsApp notification for non-critical alert: {Severity}", request.Severity);
                return new NotificationResponseDto
                {
                    NotificationId = Guid.NewGuid(),
                    SentAt = DateTime.UtcNow,
                    Channel = NotificationChannel.WhatsApp,
                    Success = true,
                    RecipientsCount = 0
                };
            }

            var response = new NotificationResponseDto
            {
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.WhatsApp,
                RecipientsCount = request.Recipients.Count
            };

            try
            {
                var messagesSent = 0;

                foreach (var recipient in request.Recipients)
                {
                    try
                    {
                        var message = CreateWhatsAppMessage(request);
                        await SendWhatsAppMessageAsync(recipient, message);

                        messagesSent++;
                        _logger.LogInformation("WhatsApp message sent successfully to {Recipient}", recipient);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending WhatsApp message to {Recipient}", recipient);
                    }
                }

                response.Success = messagesSent > 0;
                if (messagesSent < request.Recipients.Count)
                {
                    response.ErrorMessage = $"{messagesSent} de {request.Recipients.Count} mensajes enviados exitosamente";
                }

                _logger.LogInformation("WhatsApp notification completed: {MessagesSent}/{TotalRecipients} messages sent", messagesSent, request.Recipients.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error sending WhatsApp notification");
                throw;
            }

            return response;
        }

        /// <summary>
        /// Envía un mensaje individual de WhatsApp usando Twilio
        /// </summary>
        private async Task SendWhatsAppMessageAsync(string toPhoneNumber, string message)
        {
            // Formatear número de teléfono para WhatsApp
            var formattedTo = FormatPhoneNumberForWhatsApp(toPhoneNumber);

            var messageOptions = new CreateMessageOptions(
                new PhoneNumber($"whatsapp:{formattedTo}"))
            {
                From = new PhoneNumber($"whatsapp:{_fromPhoneNumber}"),
                Body = message
            };

            var messageResource = await MessageResource.CreateAsync(messageOptions);

            if (messageResource.Status != MessageResource.StatusEnum.Queued &&
                messageResource.Status != MessageResource.StatusEnum.Sent)
            {
                throw new Exception($"Twilio message failed with status: {messageResource.Status}");
            }
        }

        /// <summary>
        /// Crea el mensaje formateado para WhatsApp
        /// </summary>
        private string CreateWhatsAppMessage(NotificationRequestDto request)
        {
            var severityEmoji = request.Severity switch
            {
                NotificationSeverity.Critical => "🚨",
                NotificationSeverity.Warning => "⚠️",
                NotificationSeverity.Info => "📊",
                _ => "📢"
            };

            var sb = new StringBuilder();
            sb.AppendLine($"{severityEmoji} {request.Title}");
            sb.AppendLine();
            sb.AppendLine(request.Message);

            // Agregar metadata formateada si existe
            if (request.Metadata != null && request.Metadata.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("📋 Información adicional:");

                foreach (var metadata in request.Metadata.Take(5)) // Máximo 5 campos para no hacer muy largo
                {
                    sb.AppendLine($"• {metadata.Key}: {metadata.Value}");
                }
            }

            // Agregar información de contacto
            sb.AppendLine();
            sb.AppendLine($"Sistema Monitor Impresoras - {DateTime.UtcNow:dd/MM/yy HH:mm}");

            if (request.RequireAcknowledgment)
            {
                sb.AppendLine();
                sb.AppendLine("🔗 Marcar como resuelto: [Enlace de reconocimiento]");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formatea un número de teléfono para WhatsApp
        /// </summary>
        private string FormatPhoneNumberForWhatsApp(string phoneNumber)
        {
            // Remover caracteres no numéricos excepto el signo +
            var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // Asegurar que tenga el formato internacional correcto
            if (cleaned.StartsWith("+"))
            {
                return cleaned;
            }

            // Agregar código de país si no lo tiene (ejemplo: España)
            if (cleaned.Length == 9) // Número local español
            {
                return $"+34{cleaned}";
            }

            // Para otros países, asumir que ya está en formato internacional
            return $"+{cleaned}";
        }

        /// <summary>
        /// Prueba la configuración de WhatsApp
        /// </summary>
        public async Task<bool> TestWhatsAppConfigurationAsync()
        {
            try
            {
                // Solo probar con mensajes críticos para evitar spam en pruebas
                var testResponse = await SendCriticalAsync(
                    "🧪 Prueba de WhatsApp",
                    $"Prueba de configuración de WhatsApp realizada el {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}"
                );

                return testResponse.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing WhatsApp configuration");
                return false;
            }
        }

        /// <summary>
        /// Envía alerta crítica por WhatsApp (solo para casos realmente críticos)
        /// </summary>
        public async Task<NotificationResponseDto> SendCriticalAlertToWhatsAppAsync(
            string title,
            string message,
            List<string> phoneNumbers,
            Dictionary<string, object>? metadata = null)
        {
            var request = new NotificationRequestDto
            {
                Title = title,
                Message = message,
                Severity = NotificationSeverity.Critical,
                Recipients = phoneNumbers,
                Channels = new List<NotificationChannel> { NotificationChannel.WhatsApp },
                RequireAcknowledgment = true,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            return (await SendNotificationAsync(request)).FirstOrDefault() ?? new NotificationResponseDto();
        }

        /// <summary>
        /// Obtiene estadísticas de uso de WhatsApp
        /// </summary>
        public async Task<WhatsAppStatisticsDto> GetWhatsAppStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                // Aquí haría consultas a Twilio API para obtener estadísticas reales
                return new WhatsAppStatisticsDto
                {
                    MessagesSent = 0,
                    MessagesDelivered = 0,
                    MessagesFailed = 0,
                    MessagesRead = 0,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting WhatsApp statistics");
                return new WhatsAppStatisticsDto();
            }
        }
    }

    /// <summary>
    /// DTO para estadísticas de WhatsApp
    /// </summary>
    public class WhatsAppStatisticsDto
    {
        public int MessagesSent { get; set; }
        public int MessagesDelivered { get; set; }
        public int MessagesFailed { get; set; }
        public int MessagesRead { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
