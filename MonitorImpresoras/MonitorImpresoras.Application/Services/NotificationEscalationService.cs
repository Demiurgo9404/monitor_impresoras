using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using System.Text.Json;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para gestión de escalamiento automático de notificaciones
    /// </summary>
    public class NotificationEscalationService : INotificationEscalationService
    {
        private readonly INotificationService _notificationService;
        private readonly IExtendedAuditService _auditService;
        private readonly ILogger<NotificationEscalationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public NotificationEscalationService(
            INotificationService notificationService,
            IExtendedAuditService auditService,
            ILogger<NotificationEscalationService> logger,
            IServiceProvider serviceProvider)
        {
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Inicia el seguimiento de escalamiento para una notificación crítica
        /// </summary>
        public async Task StartEscalationTrackingAsync(
            string notificationId,
            List<string> originalRecipients,
            NotificationSeverity severity,
            Dictionary<string, object>? metadata = null)
        {
            try
            {
                if (severity != NotificationSeverity.Critical)
                {
                    _logger.LogInformation("Notification {NotificationId} is not critical, skipping escalation tracking", notificationId);
                    return;
                }

                // Crear registro inicial de escalamiento
                var escalationRecord = new NotificationEscalationHistory
                {
                    NotificationId = notificationId,
                    EscalationLevel = 1,
                    Channel = "Email",
                    OriginalRecipients = JsonSerializer.Serialize(originalRecipients),
                    EscalatedRecipients = JsonSerializer.Serialize(originalRecipients),
                    EscalationReason = "Initial notification sent",
                    ResponseTimeMinutes = 15,
                    OriginalNotificationSentAt = DateTime.UtcNow,
                    EscalatedAt = DateTime.UtcNow,
                    EscalationSuccessful = true,
                    CreatedBy = "System"
                };

                // Aquí iría el código para guardar en BD
                // await _context.NotificationEscalationHistory.AddAsync(escalationRecord);
                // await _context.SaveChangesAsync();

                _logger.LogInformation("Escalation tracking started for notification {NotificationId}", notificationId);

                // Programar chequeo de reconocimiento después de 15 minutos
                await ScheduleEscalationCheckAsync(notificationId, originalRecipients, metadata, 15);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting escalation tracking for notification {NotificationId}", notificationId);
            }
        }

        /// <summary>
        /// Programa un chequeo de escalamiento para el futuro
        /// </summary>
        private async Task ScheduleEscalationCheckAsync(
            string notificationId,
            List<string> recipients,
            Dictionary<string, object>? metadata,
            int delayMinutes)
        {
            try
            {
                // Esta implementación sería más compleja con un sistema de jobs en producción
                // Por simplicidad, simulamos el proceso

                await Task.Delay(TimeSpan.FromMinutes(delayMinutes));

                await CheckEscalationStatusAsync(notificationId, recipients, metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling escalation check for notification {NotificationId}", notificationId);
            }
        }

        /// <summary>
        /// Verifica si una notificación ha sido reconocida y realiza escalamiento si es necesario
        /// </summary>
        private async Task CheckEscalationStatusAsync(
            string notificationId,
            List<string> originalRecipients,
            Dictionary<string, object>? metadata)
        {
            try
            {
                // Aquí verificaría si la notificación fue reconocida
                // Por simplicidad, asumimos que no fue reconocida y procedemos con escalamiento

                var wasAcknowledged = false; // Esto vendría de una consulta a BD

                if (!wasAcknowledged)
                {
                    await EscalateNotificationAsync(notificationId, originalRecipients, metadata, 2);
                }
                else
                {
                    _logger.LogInformation("Notification {NotificationId} was acknowledged, no escalation needed", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking escalation status for notification {NotificationId}", notificationId);
            }
        }

        /// <summary>
        /// Realiza el escalamiento de una notificación a un nivel superior
        /// </summary>
        private async Task EscalateNotificationAsync(
            string notificationId,
            List<string> originalRecipients,
            Dictionary<string, object>? metadata,
            int escalationLevel)
        {
            try
            {
                var escalatedRecipients = GetEscalatedRecipients(escalationLevel, originalRecipients);

                var escalationReason = escalationLevel switch
                {
                    2 => "No response after 15 minutes - escalating to supervisors",
                    3 => "No response after 30 minutes - escalating to IT management",
                    _ => $"No response after {escalationLevel * 15} minutes - final escalation"
                };

                // Crear registro de escalamiento
                var escalationRecord = new NotificationEscalationHistory
                {
                    NotificationId = notificationId,
                    EscalationLevel = escalationLevel,
                    Channel = "Email",
                    OriginalRecipients = JsonSerializer.Serialize(originalRecipients),
                    EscalatedRecipients = JsonSerializer.Serialize(escalatedRecipients),
                    EscalationReason = escalationReason,
                    ResponseTimeMinutes = escalationLevel * 15,
                    OriginalNotificationSentAt = DateTime.UtcNow.AddMinutes(-(escalationLevel - 1) * 15),
                    EscalatedAt = DateTime.UtcNow,
                    EscalationSuccessful = true,
                    CreatedBy = "System"
                };

                // Aquí iría el código para guardar en BD

                // Reenviar notificación a destinatarios escalados
                var request = new NotificationRequestDto
                {
                    Title = "🚨 ESCALAMIENTO: Alerta Crítica No Atendida",
                    Message = $"La alerta crítica anterior no ha sido atendida después de {escalationLevel * 15} minutos.\n\n{metadata?["OriginalMessage"] ?? "Mensaje original no disponible"}",
                    Severity = NotificationSeverity.Critical,
                    Recipients = escalatedRecipients,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    RequireAcknowledgment = true,
                    Metadata = new Dictionary<string, object>
                    {
                        { "EscalationLevel", escalationLevel },
                        { "OriginalNotificationId", notificationId },
                        { "EscalationReason", escalationReason },
                        { "OriginalRecipients", originalRecipients }
                    }
                };

                var responses = await _notificationService.SendNotificationAsync(request);

                _logger.LogWarning("Notification {NotificationId} escalated to level {EscalationLevel} with {RecipientCount} recipients",
                    notificationId, escalationLevel, escalatedRecipients.Count);

                // Si es el último nivel de escalamiento, no programar más chequeos
                if (escalationLevel < 3)
                {
                    await ScheduleEscalationCheckAsync(notificationId, escalatedRecipients, metadata, escalationLevel * 15);
                }
                else
                {
                    _logger.LogCritical("Notification {NotificationId} reached maximum escalation level (3)", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating notification {NotificationId} to level {EscalationLevel}", notificationId, escalationLevel);
            }
        }

        /// <summary>
        /// Obtiene los destinatarios para un nivel específico de escalamiento
        /// </summary>
        private List<string> GetEscalatedRecipients(int escalationLevel, List<string> originalRecipients)
        {
            return escalationLevel switch
            {
                2 => new List<string> { "supervisor@empresa.com", "it-supervisor@empresa.com" },
                3 => new List<string> { "manager@empresa.com", "it-manager@empresa.com", "director@empresa.com" },
                _ => originalRecipients
            };
        }

        /// <summary>
        /// Marca una notificación como reconocida
        /// </summary>
        public async Task<bool> AcknowledgeNotificationAsync(string notificationId, string userId, string? comments = null)
        {
            try
            {
                // Aquí actualizaría el registro en BD para marcar como reconocido

                await _auditService.LogSystemEventAsync(
                    "notification_acknowledged",
                    $"Notification {notificationId} acknowledged by {userId}",
                    $"User: {userId}, Comments: {comments ?? "No comments"}",
                    new Dictionary<string, object>
                    {
                        { "NotificationId", notificationId },
                        { "AcknowledgedBy", userId },
                        { "Comments", comments ?? "" },
                        { "AcknowledgedAt", DateTime.UtcNow }
                    },
                    "Info",
                    true
                );

                _logger.LogInformation("Notification {NotificationId} acknowledged by {UserId}", notificationId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging notification {NotificationId} by {UserId}", notificationId, userId);
                return false;
            }
        }

        /// <summary>
        /// Obtiene el historial de escalamiento de una notificación
        /// </summary>
        public async Task<IEnumerable<NotificationEscalationHistory>> GetEscalationHistoryAsync(string notificationId)
        {
            try
            {
                // Aquí haría la consulta a BD
                // return await _context.NotificationEscalationHistory
                //     .Where(neh => neh.NotificationId == notificationId)
                //     .OrderBy(neh => neh.EscalationLevel)
                //     .ToListAsync();

                return new List<NotificationEscalationHistory>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting escalation history for notification {NotificationId}", notificationId);
                return new List<NotificationEscalationHistory>();
            }
        }

        /// <summary>
        /// Obtiene estadísticas de escalamiento
        /// </summary>
        public async Task<EscalationStatisticsDto> GetEscalationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                // Aquí haría consultas de estadísticas a BD
                return new EscalationStatisticsDto
                {
                    TotalEscalations = 0,
                    EscalationsByLevel = new Dictionary<int, int>(),
                    AverageResponseTimeMinutes = 0,
                    EscalationsAcknowledged = 0,
                    EscalationsPending = 0,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting escalation statistics");
                return new EscalationStatisticsDto();
            }
        }
    }

    /// <summary>
    /// DTO para estadísticas de escalamiento
    /// </summary>
    public class EscalationStatisticsDto
    {
        public int TotalEscalations { get; set; }
        public Dictionary<int, int> EscalationsByLevel { get; set; } = new();
        public double AverageResponseTimeMinutes { get; set; }
        public int EscalationsAcknowledged { get; set; }
        public int EscalationsPending { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
