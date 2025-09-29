using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio principal de notificaciones que coordina múltiples canales
    /// </summary>
    public class MultiChannelNotificationService : INotificationService
    {
        private readonly EmailNotificationService _emailService;
        private readonly TeamsNotificationService _teamsService;
        private readonly SlackNotificationService _slackService;
        private readonly WhatsAppNotificationService _whatsAppService;
        private readonly INotificationEscalationService _escalationService;
        private readonly IExtendedAuditService _auditService;
        private readonly ILogger<MultiChannelNotificationService> _logger;

        public MultiChannelNotificationService(
            EmailNotificationService emailService,
            TeamsNotificationService teamsService,
            SlackNotificationService slackService,
            WhatsAppNotificationService whatsAppService,
            INotificationEscalationService escalationService,
            IExtendedAuditService auditService,
            ILogger<MultiChannelNotificationService> logger)
        {
            _emailService = emailService;
            _teamsService = teamsService;
            _slackService = slackService;
            _whatsAppService = whatsAppService;
            _escalationService = escalationService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<NotificationResponseDto> SendCriticalAsync(string title, string message, List<string>? recipients = null)
        {
            var request = new NotificationRequestDto
            {
                Title = title,
                Message = message,
                Severity = NotificationSeverity.Critical,
                Recipients = recipients ?? GetDefaultRecipients(),
                Channels = GetEnabledChannels(),
                RequireAcknowledgment = true,
                Metadata = new Dictionary<string, object>
                {
                    { "AlertType", "Critical" },
                    { "Timestamp", DateTime.UtcNow },
                    { "Urgency", "High" }
                }
            };

            var responses = await SendNotificationAsync(request);

            // Iniciar seguimiento de escalamiento para alertas críticas
            if (responses.Any(r => r.Success))
            {
                await _escalationService.StartEscalationTrackingAsync(
                    responses.First().NotificationId.ToString(),
                    recipients ?? GetDefaultRecipients(),
                    NotificationSeverity.Critical,
                    request.Metadata
                );
            }

            return responses.FirstOrDefault() ?? new NotificationResponseDto();
        }

        public async Task<NotificationResponseDto> SendWarningAsync(string title, string message, List<string>? recipients = null)
        {
            var request = new NotificationRequestDto
            {
                Title = title,
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

            var responses = await SendNotificationAsync(request);
            return responses.FirstOrDefault() ?? new NotificationResponseDto();
        }

        public async Task<NotificationResponseDto> SendInfoAsync(string title, string message, List<string>? recipients = null)
        {
            var request = new NotificationRequestDto
            {
                Title = title,
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

            var responses = await SendNotificationAsync(request);
            return responses.FirstOrDefault() ?? new NotificationResponseDto();
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
            return Task.FromResult<IEnumerable<NotificationHistoryDto>>(new List<NotificationHistoryDto>());
        }

        public async Task<bool> AcknowledgeNotificationAsync(Guid notificationId, string userId)
        {
            return await _escalationService.AcknowledgeNotificationAsync(
                notificationId.ToString(),
                userId,
                "Notification acknowledged via dashboard"
            );
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
                Title = "🧪 Test Multi-Canal",
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
        /// Envía notificación por un canal específico usando el servicio apropiado
        /// </summary>
        private async Task<NotificationResponseDto> SendByChannelAsync(NotificationRequestDto request, NotificationChannel channel)
        {
            return channel switch
            {
                NotificationChannel.Email => await _emailService.SendByChannelAsync(request, channel),
                NotificationChannel.Teams => await _teamsService.SendByChannelAsync(request, channel),
                NotificationChannel.Slack => await _slackService.SendByChannelAsync(request, channel),
                NotificationChannel.WhatsApp => await _whatsAppService.SendByChannelAsync(request, channel),
                _ => throw new NotSupportedException($"Canal {channel} no soportado")
            };
        }

        /// <summary>
        /// Obtiene los canales habilitados desde configuración
        /// </summary>
        private List<NotificationChannel> GetEnabledChannels()
        {
            var enabledChannels = new List<NotificationChannel>();

            // Email siempre habilitado por defecto
            enabledChannels.Add(NotificationChannel.Email);

            // Agregar otros canales si están configurados
            var config = _emailService.GetConfiguration();

            if (bool.Parse(config["Notifications:Slack:Enabled"] ?? "false"))
                enabledChannels.Add(NotificationChannel.Slack);

            if (bool.Parse(config["Notifications:Teams:Enabled"] ?? "false"))
                enabledChannels.Add(NotificationChannel.Teams);

            if (bool.Parse(config["Notifications:WhatsApp:Enabled"] ?? "false"))
                enabledChannels.Add(NotificationChannel.WhatsApp);

            return enabledChannels;
        }

        /// <summary>
        /// Obtiene los destinatarios por defecto desde configuración
        /// </summary>
        private List<string> GetDefaultRecipients()
        {
            var config = _emailService.GetConfiguration();
            var recipients = config["Notifications:DefaultRecipients"];

            if (string.IsNullOrEmpty(recipients))
                return new List<string> { "admin@monitorimpresoras.com" };

            return recipients.Split(',').Select(r => r.Trim()).ToList();
        }
    }

    /// <summary>
    /// Extensiones para acceder a configuración desde servicios
    /// </summary>
    public static class NotificationServiceExtensions
    {
        public static IConfiguration GetConfiguration(this BaseNotificationService service)
        {
            // Usar reflexión para acceder al campo privado _configuration
            var field = typeof(BaseNotificationService).GetField("_configuration",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return (IConfiguration)field?.GetValue(service)!;
        }
    }
}
