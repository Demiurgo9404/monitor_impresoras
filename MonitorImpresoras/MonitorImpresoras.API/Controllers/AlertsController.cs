using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;
        private readonly INotificationService _notificationService;
        private readonly INotificationEscalationService _escalationService;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(
            IAlertService alertService,
            INotificationService notificationService,
            INotificationEscalationService escalationService,
            ILogger<AlertsController> logger)
        {
            _alertService = alertService;
            _notificationService = notificationService;
            _escalationService = escalationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el dashboard de alertas con estadísticas y estado actual
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAlertsDashboard()
        {
            try
            {
                _logger.LogInformation("Obteniendo dashboard de alertas");

                var dashboard = new
                {
                    Timestamp = DateTime.UtcNow,
                    Summary = await GetAlertsSummaryAsync(),
                    RecentAlerts = await GetRecentAlertsAsync(),
                    EscalationStatus = await GetEscalationStatusAsync(),
                    ChannelStatus = await GetChannelStatusAsync()
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo dashboard de alertas");
                throw;
            }
        }

        /// <summary>
        /// Envía una alerta de prueba
        /// </summary>
        [HttpPost("test")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SendTestAlert([FromBody] TestAlertDto testAlert)
        {
            try
            {
                _logger.LogInformation("Enviando alerta de prueba");

                var responses = await _notificationService.TestNotificationConfigurationAsync(testAlert.Channels);

                return Ok(new
                {
                    Success = responses.All(r => r.Success),
                    TotalChannels = responses.Count,
                    SuccessfulChannels = responses.Count(r => r.Success),
                    FailedChannels = responses.Count(r => !r.Success),
                    Results = responses.Select(r => new
                    {
                        Channel = r.Channel,
                        Success = r.Success,
                        RecipientsCount = r.RecipientsCount,
                        ErrorMessage = r.ErrorMessage
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando alerta de prueba");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de escalamiento
        /// </summary>
        [HttpGet("escalation-statistics")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetEscalationStatistics(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de escalamiento");

                var statistics = await _escalationService.GetEscalationStatisticsAsync(fromDate, toDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de escalamiento");
                throw;
            }
        }

        /// <summary>
        /// Marca una notificación como reconocida
        /// </summary>
        [HttpPut("{notificationId}/acknowledge")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AcknowledgeAlert(
            Guid notificationId,
            [FromBody] AcknowledgeAlertDto acknowledgment)
        {
            try
            {
                _logger.LogInformation("Reconociendo alerta {NotificationId}", notificationId);

                var success = await _notificationService.AcknowledgeNotificationAsync(
                    notificationId,
                    User.Identity?.Name ?? "unknown"
                );

                if (!success)
                {
                    return NotFound($"Alerta {notificationId} no encontrada");
                }

                return Ok(new
                {
                    Message = "Alerta reconocida exitosamente",
                    AcknowledgedBy = User.Identity?.Name,
                    AcknowledgedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconociendo alerta {NotificationId}", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene el estado de todos los canales de notificación
        /// </summary>
        private async Task<object> GetChannelStatusAsync()
        {
            var channelTests = await _notificationService.TestNotificationConfigurationAsync(
                new List<NotificationChannel>
                {
                    NotificationChannel.Email,
                    NotificationChannel.Slack,
                    NotificationChannel.Teams,
                    NotificationChannel.WhatsApp
                });

            return new
            {
                Email = new { Enabled = true, Status = "Active", LastTest = DateTime.UtcNow },
                Slack = new { Enabled = channelTests.Any(r => r.Channel == NotificationChannel.Slack && r.Success), Status = channelTests.Any(r => r.Channel == NotificationChannel.Slack && r.Success) ? "Active" : "Inactive", LastTest = DateTime.UtcNow },
                Teams = new { Enabled = channelTests.Any(r => r.Channel == NotificationChannel.Teams && r.Success), Status = channelTests.Any(r => r.Channel == NotificationChannel.Teams && r.Success) ? "Active" : "Inactive", LastTest = DateTime.UtcNow },
                WhatsApp = new { Enabled = channelTests.Any(r => r.Channel == NotificationChannel.WhatsApp && r.Success), Status = channelTests.Any(r => r.Channel == NotificationChannel.WhatsApp && r.Success) ? "Active" : "Inactive", LastTest = DateTime.UtcNow }
            };
        }

        /// <summary>
        /// Obtiene resumen de alertas
        /// </summary>
        private async Task<object> GetAlertsSummaryAsync()
        {
            // Esta información vendría de consultas a BD en producción
            return new
            {
                TotalAlertsToday = 12,
                CriticalAlerts = 2,
                WarningAlerts = 5,
                InfoAlerts = 5,
                AcknowledgedAlerts = 10,
                PendingAlerts = 2,
                AverageResponseTime = "8 minutos"
            };
        }

        /// <summary>
        /// Obtiene alertas recientes
        /// </summary>
        private async Task<IEnumerable<object>> GetRecentAlertsAsync()
        {
            // Esta información vendría de consultas a BD en producción
            return new[]
            {
                new
                {
                    Id = "alert-001",
                    Type = "Critical",
                    Title = "Impresora Desconectada",
                    Message = "HP LaserJet Pro se desconectó",
                    Channel = "Email",
                    SentAt = DateTime.UtcNow.AddMinutes(-15),
                    Status = "Sent",
                    Acknowledged = false
                },
                new
                {
                    Id = "alert-002",
                    Type = "Warning",
                    Title = "Tóner Bajo",
                    Message = "Impresora Marketing tóner al 12%",
                    Channel = "Teams",
                    SentAt = DateTime.UtcNow.AddMinutes(-30),
                    Status = "Sent",
                    Acknowledged = true
                },
                new
                {
                    Id = "alert-003",
                    Type = "Info",
                    Title = "Reporte Diario",
                    Message = "Reporte diario enviado exitosamente",
                    Channel = "Slack",
                    SentAt = DateTime.UtcNow.AddHours(-2),
                    Status = "Sent",
                    Acknowledged = false
                }
            };
        }

        /// <summary>
        /// Obtiene estado de escalamiento
        /// </summary>
        private async Task<object> GetEscalationStatusAsync()
        {
            // Esta información vendría de consultas a BD en producción
            return new
            {
                ActiveEscalations = 1,
                EscalationsToday = 3,
                AverageEscalationTime = "12 minutos",
                EscalationsAcknowledged = 2,
                EscalationsPending = 1
            };
        }
    }

    /// <summary>
    /// DTO para alerta de prueba
    /// </summary>
    public class TestAlertDto
    {
        public List<NotificationChannel> Channels { get; set; } = new();
        public string? CustomMessage { get; set; }
    }

    /// <summary>
    /// DTO para reconocimiento de alerta
    /// </summary>
    public class AcknowledgeAlertDto
    {
        public string? Comments { get; set; }
    }
}
