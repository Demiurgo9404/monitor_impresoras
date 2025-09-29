using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de notificaciones para Slack usando Incoming Webhooks
    /// </summary>
    public class SlackNotificationService : BaseNotificationService
    {
        private readonly HttpClient _httpClient;

        public SlackNotificationService(
            IConfiguration configuration,
            ILogger<SlackNotificationService> logger,
            IExtendedAuditService auditService) : base(configuration, logger, auditService)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        protected override async Task<NotificationResponseDto> SendByChannelAsync(NotificationRequestDto request, NotificationChannel channel)
        {
            if (channel != NotificationChannel.Slack)
            {
                throw new NotSupportedException($"Canal {channel} no soportado por {nameof(SlackNotificationService)}");
            }

            var webhookUrl = _configuration["Notifications:Slack:WebhookUrl"];
            if (string.IsNullOrEmpty(webhookUrl))
            {
                throw new InvalidOperationException("Slack webhook URL no configurada");
            }

            var response = new NotificationResponseDto
            {
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.Slack,
                RecipientsCount = request.Recipients.Count
            };

            try
            {
                var slackMessage = CreateSlackMessage(request);

                var jsonContent = JsonSerializer.Serialize(slackMessage);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.PostAsync(webhookUrl, content);

                if (httpResponse.IsSuccessStatusCode)
                {
                    response.Success = true;
                    _logger.LogInformation("Slack notification sent successfully to {RecipientsCount} recipients", request.Recipients.Count);
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    response.Success = false;
                    response.ErrorMessage = $"HTTP {httpResponse.StatusCode}: {errorContent}";
                    _logger.LogError("Failed to send Slack notification: {Error}", response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error sending Slack notification");
                throw;
            }

            return response;
        }

        /// <summary>
        /// Crea el mensaje formateado para Slack usando Blocks
        /// </summary>
        private object CreateSlackMessage(NotificationRequestDto request)
        {
            var color = request.Severity switch
            {
                NotificationSeverity.Critical => "#FF0000", // Rojo para críticas
                NotificationSeverity.Warning => "#FFA500",  // Naranja para advertencias
                NotificationSeverity.Info => "#36a64f",     // Verde para informativas
                _ => "#808080"                              // Gris por defecto
            };

            var severityEmoji = request.Severity switch
            {
                NotificationSeverity.Critical => ":rotating_light:",
                NotificationSeverity.Warning => ":warning:",
                NotificationSeverity.Info => ":bar_chart:",
                _ => ":speaker:"
            };

            var blocks = new List<object>();

            // Header block
            blocks.Add(new
            {
                type = "header",
                text = new
                {
                    type = "plain_text",
                    text = $"{severityEmoji} {request.Title}"
                }
            });

            // Main message block
            blocks.Add(new
            {
                type = "section",
                text = new
                {
                    type = "mrkdwn",
                    text = request.Message
                }
            });

            // Metadata fields si existe información adicional
            if (request.Metadata != null && request.Metadata.Count > 0)
            {
                var fields = new List<object>();

                foreach (var metadata in request.Metadata.Take(10)) // Máximo 10 campos
                {
                    fields.Add(new
                    {
                        type = "mrkdwn",
                        text = $"*{metadata.Key}:*\n{metadata.Value}"
                    });
                }

                blocks.Add(new
                {
                    type = "section",
                    fields = fields
                });
            }

            // Action buttons si requiere reconocimiento
            if (request.RequireAcknowledgment)
            {
                blocks.Add(new
                {
                    type = "actions",
                    elements = new[]
                    {
                        new
                        {
                            type = "button",
                            text = new
                            {
                                type = "plain_text",
                                text = "✅ Marcar como Resuelto"
                            },
                            style = "primary",
                            url = $"{_configuration["Application:BaseUrl"]}/notifications/{Guid.NewGuid()}/acknowledge"
                        }
                    }
                });
            }

            // Footer con información del sistema
            blocks.Add(new
            {
                type = "context",
                elements = new[]
                {
                    new
                    {
                        type = "mrkdwn",
                        text = $"*Sistema de Monitor de Impresoras* • {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}"
                    }
                }
            });

            return new
            {
                blocks = blocks,
                attachments = new[]
                {
                    new
                    {
                        color = color,
                        footer = "Sistema Monitor Impresoras",
                        ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    }
                }
            };
        }

        /// <summary>
        /// Prueba la configuración de Slack
        /// </summary>
        public async Task<bool> TestSlackConfigurationAsync()
        {
            try
            {
                var testResponse = await SendInfoAsync(
                    "🧪 Prueba de Slack",
                    $"Prueba de configuración de Slack realizada el {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}"
                );

                return testResponse.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Slack configuration");
                return false;
            }
        }

        /// <summary>
        /// Envía reporte diario a Slack
        /// </summary>
        public async Task<NotificationResponseDto> SendDailyReportToSlackAsync(
            string reportTitle,
            string reportContent,
            Dictionary<string, object>? metadata = null)
        {
            var request = new NotificationRequestDto
            {
                Title = $"📊 Reporte Diario - {DateTime.UtcNow:dd/MM/yyyy}",
                Message = reportContent,
                Severity = NotificationSeverity.Info,
                Recipients = new List<string> { "slack-channel" },
                Channels = new List<NotificationChannel> { NotificationChannel.Slack },
                RequireAcknowledgment = false,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            return (await SendNotificationAsync(request)).FirstOrDefault() ?? new NotificationResponseDto();
        }
    }
}
