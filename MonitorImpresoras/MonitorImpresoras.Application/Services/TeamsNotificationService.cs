using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de notificaciones para Microsoft Teams usando Incoming Webhooks
    /// </summary>
    public class TeamsNotificationService : BaseNotificationService
    {
        private readonly HttpClient _httpClient;

        public TeamsNotificationService(
            IConfiguration configuration,
            ILogger<TeamsNotificationService> logger,
            IExtendedAuditService auditService) : base(configuration, logger, auditService)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        protected override async Task<NotificationResponseDto> SendByChannelAsync(NotificationRequestDto request, NotificationChannel channel)
        {
            if (channel != NotificationChannel.Teams)
            {
                throw new NotSupportedException($"Canal {channel} no soportado por {nameof(TeamsNotificationService)}");
            }

            var webhookUrl = _configuration["Notifications:Teams:WebhookUrl"];
            if (string.IsNullOrEmpty(webhookUrl))
            {
                throw new InvalidOperationException("Teams webhook URL no configurada");
            }

            var response = new NotificationResponseDto
            {
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.Teams,
                RecipientsCount = request.Recipients.Count
            };

            try
            {
                var teamsMessage = CreateTeamsMessage(request);

                var jsonContent = JsonSerializer.Serialize(teamsMessage);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync(webhookUrl, content);

                if (httpResponse.IsSuccessStatusCode)
                {
                    response.Success = true;
                    _logger.LogInformation("Teams notification sent successfully to {RecipientsCount} recipients", request.Recipients.Count);
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    response.Success = false;
                    response.ErrorMessage = $"HTTP {httpResponse.StatusCode}: {errorContent}";
                    _logger.LogError("Failed to send Teams notification: {Error}", response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error sending Teams notification");
                throw;
            }

            return response;
        }

        /// <summary>
        /// Crea el mensaje formateado para Microsoft Teams usando Adaptive Cards
        /// </summary>
        private object CreateTeamsMessage(NotificationRequestDto request)
        {
            var themeColor = request.Severity switch
            {
                NotificationSeverity.Critical => "FF0000", // Rojo para críticas
                NotificationSeverity.Warning => "FFA500",  // Naranja para advertencias
                NotificationSeverity.Info => "0078D4",     // Azul para informativas
                _ => "808080"                              // Gris por defecto
            };

            var severityEmoji = request.Severity switch
            {
                NotificationSeverity.Critical => "🚨",
                NotificationSeverity.Warning => "⚠️",
                NotificationSeverity.Info => "📊",
                _ => "📢"
            };

            var facts = new List<object>();

            // Agregar metadata como facts si existe
            if (request.Metadata != null)
            {
                foreach (var metadata in request.Metadata)
                {
                    facts.Add(new
                    {
                        name = metadata.Key,
                        value = metadata.Value.ToString() ?? "N/A"
                    });
                }
            }

            // Agregar información de destinatarios si es relevante
            if (request.Recipients.Count > 0)
            {
                facts.Add(new
                {
                    name = "Destinatarios",
                    value = string.Join(", ", request.Recipients)
                });
            }

            return new
            {
                type = "message",
                attachments = new[]
                {
                    new
                    {
                        contentType = "application/vnd.microsoft.card.adaptive",
                        contentUrl = (string?)null,
                        content = new
                        {
                            type = "AdaptiveCard",
                            version = "1.4",
                            body = new[]
                            {
                                new
                                {
                                    type = "TextBlock",
                                    text = $"{severityEmoji} {request.Title}",
                                    weight = "Bolder",
                                    size = "Large",
                                    color = "Attention"
                                },
                                new
                                {
                                    type = "TextBlock",
                                    text = request.Message,
                                    wrap = true
                                },
                                new
                                {
                                    type = "FactSet",
                                    facts = facts
                                }
                            },
                            actions = request.RequireAcknowledgment ? new[]
                            {
                                new
                                {
                                    type = "Action.OpenUrl",
                                    title = "Marcar como Resuelto",
                                    url = $"{_configuration["Application:BaseUrl"]}/notifications/{Guid.NewGuid()}/acknowledge"
                                }
                            } : Array.Empty<object>()
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Prueba la configuración de Teams
        /// </summary>
        public async Task<bool> TestTeamsConfigurationAsync()
        {
            try
            {
                var testResponse = await SendInfoAsync(
                    "🧪 Prueba de Teams",
                    $"Prueba de configuración de Teams realizada el {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}"
                );

                return testResponse.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Teams configuration");
                return false;
            }
        }

        /// <summary>
        /// Envía reporte diario a Teams
        /// </summary>
        public async Task<NotificationResponseDto> SendDailyReportToTeamsAsync(
            string reportTitle,
            string reportContent,
            Dictionary<string, object>? metadata = null)
        {
            var request = new NotificationRequestDto
            {
                Title = $"📊 Reporte Diario - {DateTime.UtcNow:dd/MM/yyyy}",
                Message = reportContent,
                Severity = NotificationSeverity.Info,
                Recipients = new List<string> { "teams-channel" },
                Channels = new List<NotificationChannel> { NotificationChannel.Teams },
                RequireAcknowledgment = false,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            return (await SendNotificationAsync(request)).FirstOrDefault() ?? new NotificationResponseDto();
        }
    }
}
