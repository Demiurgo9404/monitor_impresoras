using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Envía una notificación crítica manual
        /// </summary>
        [HttpPost("critical")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendCriticalNotification(
            [FromBody] ManualNotificationDto notification)
        {
            try
            {
                _logger.LogInformation("Enviando notificación crítica manual");

                var response = await _notificationService.SendCriticalAsync(
                    notification.Title,
                    notification.Message,
                    notification.Recipients
                );

                return Ok(new
                {
                    Success = response.Success,
                    NotificationId = response.NotificationId,
                    RecipientsCount = response.RecipientsCount,
                    SentAt = response.SentAt,
                    Channel = response.Channel,
                    ErrorMessage = response.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación crítica manual");
                throw;
            }
        }

        /// <summary>
        /// Envía una notificación de advertencia manual
        /// </summary>
        [HttpPost("warning")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendWarningNotification(
            [FromBody] ManualNotificationDto notification)
        {
            try
            {
                _logger.LogInformation("Enviando notificación de advertencia manual");

                var response = await _notificationService.SendWarningAsync(
                    notification.Title,
                    notification.Message,
                    notification.Recipients
                );

                return Ok(new
                {
                    Success = response.Success,
                    NotificationId = response.NotificationId,
                    RecipientsCount = response.RecipientsCount,
                    SentAt = response.SentAt,
                    Channel = response.Channel,
                    ErrorMessage = response.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de advertencia manual");
                throw;
            }
        }

        /// <summary>
        /// Envía una notificación informativa manual
        /// </summary>
        [HttpPost("info")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendInfoNotification(
            [FromBody] ManualNotificationDto notification)
        {
            try
            {
                _logger.LogInformation("Enviando notificación informativa manual");

                var response = await _notificationService.SendInfoAsync(
                    notification.Title,
                    notification.Message,
                    notification.Recipients
                );

                return Ok(new
                {
                    Success = response.Success,
                    NotificationId = response.NotificationId,
                    RecipientsCount = response.RecipientsCount,
                    SentAt = response.SentAt,
                    Channel = response.Channel,
                    ErrorMessage = response.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación informativa manual");
                throw;
            }
        }

        /// <summary>
        /// Envía una notificación personalizada con múltiples canales
        /// </summary>
        [HttpPost("custom")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendCustomNotification(
            [FromBody] CustomNotificationDto notification)
        {
            try
            {
                _logger.LogInformation("Enviando notificación personalizada");

                var request = new NotificationRequestDto
                {
                    Title = notification.Title,
                    Message = notification.Message,
                    Severity = notification.Severity,
                    Recipients = notification.Recipients,
                    Channels = notification.Channels,
                    RequireAcknowledgment = notification.RequireAcknowledgment,
                    Metadata = notification.Metadata
                };

                var responses = await _notificationService.SendNotificationAsync(request);

                return Ok(new
                {
                    TotalNotifications = responses.Count,
                    Successful = responses.Count(r => r.Success),
                    Failed = responses.Count(r => !r.Success),
                    Responses = responses.Select(r => new
                    {
                        Channel = r.Channel,
                        Success = r.Success,
                        RecipientsCount = r.RecipientsCount,
                        SentAt = r.SentAt,
                        ErrorMessage = r.ErrorMessage
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación personalizada");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de notificaciones
        /// </summary>
        [HttpGet("history")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetNotificationHistory(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] NotificationSeverity? severity,
            [FromQuery] NotificationChannel? channel,
            [FromQuery] string? recipient,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Obteniendo historial de notificaciones");

                var history = await _notificationService.GetNotificationHistoryAsync(
                    fromDate, toDate, severity, channel, recipient, page, pageSize);

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de notificaciones");
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
        public async Task<IActionResult> AcknowledgeNotification(
            Guid notificationId,
            [FromBody] AcknowledgeNotificationDto acknowledgment)
        {
            try
            {
                _logger.LogInformation("Reconociendo notificación {NotificationId}", notificationId);

                var success = await _notificationService.AcknowledgeNotificationAsync(
                    notificationId,
                    User.Identity?.Name ?? "unknown"
                );

                if (!success)
                {
                    return NotFound($"Notificación {notificationId} no encontrada");
                }

                return Ok(new { Message = "Notificación reconocida exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconociendo notificación {NotificationId}", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de notificaciones
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetNotificationStatistics(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de notificaciones");

                var statistics = await _notificationService.GetNotificationStatisticsAsync(fromDate, toDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de notificaciones");
                throw;
            }
        }

        /// <summary>
        /// Prueba la configuración de notificaciones
        /// </summary>
        [HttpPost("test")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> TestNotificationConfiguration(
            [FromBody] TestNotificationDto testConfig)
        {
            try
            {
                _logger.LogInformation("Probando configuración de notificaciones");

                var responses = await _notificationService.TestNotificationConfigurationAsync(testConfig.Channels);

                return Ok(new
                {
                    TotalTests = responses.Count,
                    Successful = responses.Count(r => r.Success),
                    Failed = responses.Count(r => !r.Success),
                    TestResults = responses.Select(r => new
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
                _logger.LogError(ex, "Error probando configuración de notificaciones");
                throw;
            }
        }
    }

    /// <summary>
    /// DTO para notificaciones manuales
    /// </summary>
    public class ManualNotificationDto
    {
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public List<string>? Recipients { get; set; }
    }

    /// <summary>
    /// DTO para notificaciones personalizadas
    /// </summary>
    public class CustomNotificationDto
    {
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public NotificationSeverity Severity { get; set; }
        public List<string> Recipients { get; set; } = new();
        public List<NotificationChannel> Channels { get; set; } = new();
        public bool RequireAcknowledgment { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// DTO para reconocimiento de notificaciones
    /// </summary>
    public class AcknowledgeNotificationDto
    {
        public string UserId { get; set; } = default!;
        public string? Comments { get; set; }
    }

    /// <summary>
    /// DTO para pruebas de configuración
    /// </summary>
    public class TestNotificationDto
    {
        public List<NotificationChannel> Channels { get; set; } = new();
    }
}
