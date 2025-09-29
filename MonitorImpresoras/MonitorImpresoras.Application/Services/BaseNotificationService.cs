using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Net.Mail;
using System.Text.Json;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio base para envío de notificaciones
    /// </summary>
    public abstract class BaseNotificationService : INotificationService
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<BaseNotificationService> _logger;
        protected readonly IExtendedAuditService _auditService;

        public BaseNotificationService(
            IConfiguration configuration,
            ILogger<BaseNotificationService> logger,
            IExtendedAuditService auditService)
        {
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
        }

        public virtual async Task<NotificationResponseDto> SendCriticalAsync(string title, string message, List<string>? recipients = null, Dictionary<string, object>? metadata = null)
        {
            var request = new NotificationRequestDto
            {
                Title = $"🚨 {title}",
                Message = message,
                Severity = NotificationSeverity.Critical,
                Recipients = recipients ?? GetDefaultRecipients(),
                Channels = GetEnabledChannels(),
                RequireAcknowledgment = true,
                Metadata = metadata ?? new Dictionary<string, object>
                {
                    { "AlertType", "Critical" },
                    { "Timestamp", DateTime.UtcNow }
                }
            };

            return (await SendNotificationAsync(request)).FirstOrDefault() ?? new NotificationResponseDto();
        }

        public virtual async Task<NotificationResponseDto> SendWarningAsync(string title, string message, List<string>? recipients = null)
        {
            var request = new NotificationRequestDto
            {
                Title = $"⚠️ {title}",
                Message = message,
                Severity = NotificationSeverity.Warning,
                Recipients = recipients ?? GetDefaultRecipients(),
                Channels = GetEnabledChannels(),
                RequireAcknowledgment = false,
                Metadata = new Dictionary<string, object>
                {
                    { "AlertType", "Warning" },
                    { "Timestamp", DateTime.UtcNow }
                }
            };

            return (await SendNotificationAsync(request)).FirstOrDefault() ?? new NotificationResponseDto();
        }

        public virtual async Task<NotificationResponseDto> SendInfoAsync(string title, string message, List<string>? recipients = null)
        {
            var request = new NotificationRequestDto
            {
                Title = $"📊 {title}",
                Message = message,
                Severity = NotificationSeverity.Info,
                Recipients = recipients ?? GetDefaultRecipients(),
                Channels = GetEnabledChannels(),
                RequireAcknowledgment = false,
                Metadata = new Dictionary<string, object>
                {
                    { "AlertType", "Info" },
                    { "Timestamp", DateTime.UtcNow }
                }
            };

            return (await SendNotificationAsync(request)).FirstOrDefault() ?? new NotificationResponseDto();
        }

        public async Task<List<NotificationResponseDto>> SendNotificationAsync(NotificationRequestDto request)
        {
            var responses = new List<NotificationResponseDto>();

            foreach (var channel in request.Channels)
            {
                try
                {
                    var response = await SendByChannelAsync(request, channel);
                    responses.Add(response);

                    // Registrar evento de auditoría
                    await _auditService.LogSystemEventAsync(
                        $"notification_sent_{channel.ToString().ToLower()}",
                        $"{request.Severity} notification sent via {channel}",
                        $"Title: {request.Title}, Recipients: {string.Join(", ", request.Recipients)}",
                        new Dictionary<string, object>
                        {
                            { "Title", request.Title },
                            { "Severity", request.Severity.ToString() },
                            { "Channel", channel.ToString() },
                            { "Recipients", request.Recipients },
                            { "Success", response.Success }
                        },
                        request.Severity == NotificationSeverity.Critical ? "Warning" : "Info",
                        response.Success
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification via {Channel}", channel);

                    responses.Add(new NotificationResponseDto
                    {
                        NotificationId = Guid.NewGuid(),
                        SentAt = DateTime.UtcNow,
                        Channel = channel,
                        Success = false,
                        ErrorMessage = ex.Message,
                        RecipientsCount = request.Recipients.Count
                    });

                    // Registrar error en auditoría
                    await _auditService.LogSystemEventAsync(
                        $"notification_failed_{channel.ToString().ToLower()}",
                        $"Failed to send {request.Severity} notification via {channel}",
                        $"Error: {ex.Message}",
                        new Dictionary<string, object>
                        {
                            { "Title", request.Title },
                            { "Severity", request.Severity.ToString() },
                            { "Channel", channel.ToString() },
                            { "Recipients", request.Recipients },
                            { "Error", ex.Message }
                        },
                        "Error",
                        false
                    );
                }
            }

            return responses;
        }

        public Task<IEnumerable<NotificationHistoryDto>> GetNotificationHistoryAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            NotificationSeverity? severity = null,
            NotificationChannel? channel = null,
            string? recipient = null,
            int page = 1,
            int pageSize = 50)
        {
            // Esta implementación sería más compleja con una tabla de historial
            // Por ahora retornamos una lista vacía
            return Task.FromResult<IEnumerable<NotificationHistoryDto>>(new List<NotificationHistoryDto>());
        }

        public Task<bool> AcknowledgeNotificationAsync(Guid notificationId, string userId)
        {
            // Esta implementación sería más compleja con una tabla de historial
            return Task.FromResult(true);
        }

        public Task<NotificationStatisticsDto> GetNotificationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Esta implementación sería más compleja con una tabla de historial
            return Task.FromResult(new NotificationStatisticsDto
            {
                PeriodStart = fromDate ?? DateTime.UtcNow.AddDays(-30),
                PeriodEnd = toDate ?? DateTime.UtcNow
            });
        }

        public async Task<List<NotificationResponseDto>> TestNotificationConfigurationAsync(List<NotificationChannel> channels)
        {
            var testRequest = new NotificationRequestDto
            {
                Title = "🧪 Test de Notificaciones",
                Message = $"Mensaje de prueba enviado el {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}",
                Severity = NotificationSeverity.Info,
                Recipients = GetDefaultRecipients(),
                Channels = channels,
                Metadata = new Dictionary<string, object>
                {
                    { "Test", true },
                    { "Timestamp", DateTime.UtcNow }
                }
            };

            return await SendNotificationAsync(testRequest);
        }

        /// <summary>
        /// Envía notificación por un canal específico (implementado por clases hijas)
        /// </summary>
        protected abstract Task<NotificationResponseDto> SendByChannelAsync(NotificationRequestDto request, NotificationChannel channel);

        /// <summary>
        /// Obtiene los canales habilitados desde configuración
        /// </summary>
        protected virtual List<NotificationChannel> GetEnabledChannels()
        {
            var enabledChannels = new List<NotificationChannel>();

            if (bool.Parse(_configuration["Notifications:Email:Enabled"] ?? "true"))
                enabledChannels.Add(NotificationChannel.Email);

            if (bool.Parse(_configuration["Notifications:Slack:Enabled"] ?? "false"))
                enabledChannels.Add(NotificationChannel.Slack);

            if (bool.Parse(_configuration["Notifications:Teams:Enabled"] ?? "false"))
                enabledChannels.Add(NotificationChannel.Teams);

            if (bool.Parse(_configuration["Notifications:WhatsApp:Enabled"] ?? "false"))
                enabledChannels.Add(NotificationChannel.WhatsApp);

            return enabledChannels;
        }

        /// <summary>
        /// Obtiene los destinatarios por defecto desde configuración
        /// </summary>
        protected virtual List<string> GetDefaultRecipients()
        {
            var recipients = _configuration["Notifications:DefaultRecipients"];
            if (string.IsNullOrEmpty(recipients))
                return new List<string> { "admin@monitorimpresoras.com" };

            return recipients.Split(',').Select(r => r.Trim()).ToList();
        }

        /// <summary>
        /// Formatea el mensaje según el canal
        /// </summary>
        protected virtual string FormatMessageForChannel(string message, NotificationChannel channel, NotificationSeverity severity)
        {
            return channel switch
            {
                NotificationChannel.Slack => $"*{severity}*: {message}",
                NotificationChannel.Teams => $"**{severity}**: {message}",
                NotificationChannel.WhatsApp => $"{severity}: {message}",
                _ => message
            };
        }

        /// <summary>
        /// Crea un asunto formateado para email
        /// </summary>
        protected virtual string FormatEmailSubject(NotificationRequestDto request)
        {
            var prefix = request.Severity switch
            {
                NotificationSeverity.Critical => "🚨 CRÍTICA",
                NotificationSeverity.Warning => "⚠️ ADVERTENCIA",
                NotificationSeverity.Info => "📊 INFO",
                _ => "NOTIFICACIÓN"
            };

            return $"{prefix}: {request.Title}";
        }
    }
}
