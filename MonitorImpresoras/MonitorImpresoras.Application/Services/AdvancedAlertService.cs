using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de alertas avanzadas multi-canal para observabilidad
    /// </summary>
    public class AdvancedAlertService : IAdvancedAlertService
    {
        private readonly ILogger<AdvancedAlertService> _logger;
        private readonly ICentralizedLoggingService _loggingService;
        private readonly IAdvancedMetricsService _metricsService;

        public AdvancedAlertService(
            ILogger<AdvancedAlertService> logger,
            ICentralizedLoggingService loggingService,
            IAdvancedMetricsService metricsService)
        {
            _logger = logger;
            _loggingService = loggingService;
            _metricsService = metricsService;
        }

        /// <summary>
        /// Procesa evento y determina si requiere alerta
        /// </summary>
        public async Task<AlertResult> ProcessAlertEventAsync(AlertEvent alertEvent)
        {
            try
            {
                _logger.LogInformation("Procesando evento de alerta: {EventType} - {Severity}", alertEvent.EventType, alertEvent.Severity);

                var result = new AlertResult
                {
                    EventId = Guid.NewGuid(),
                    EventReceived = DateTime.UtcNow,
                    EventType = alertEvent.EventType,
                    Severity = alertEvent.Severity,
                    Source = alertEvent.Source,
                    Processed = false
                };

                // 1. Evaluar reglas de alerta
                var shouldAlert = await EvaluateAlertRulesAsync(alertEvent);

                if (!shouldAlert)
                {
                    result.Status = AlertStatus.Skipped;
                    result.Reason = "No cumple reglas de alerta";
                    return result;
                }

                // 2. Crear alerta estructurada
                var alert = CreateStructuredAlert(alertEvent);

                // 3. Determinar canales de notificación
                var channels = DetermineNotificationChannels(alertEvent);

                // 4. Enviar notificaciones
                result.NotificationsSent = await SendNotificationsAsync(alert, channels);

                // 5. Registrar evento de alerta
                await LogAlertEventAsync(alert, result);

                result.Processed = true;
                result.Status = AlertStatus.Sent;
                result.ProcessingTime = DateTime.UtcNow - result.EventReceived;

                _logger.LogInformation("Evento de alerta procesado: {EventId} - {Status}", result.EventId, result.Status);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento de alerta");

                return new AlertResult
                {
                    EventId = Guid.NewGuid(),
                    EventReceived = DateTime.UtcNow,
                    Status = AlertStatus.Failed,
                    Error = ex.Message,
                    Processed = false
                };
            }
        }

        /// <summary>
        /// Crea alerta dinámica basada en métricas de impresoras
        /// </summary>
        public async Task<DynamicAlert> CreateDynamicPrinterAlertAsync(PrinterMetrics metrics)
        {
            try
            {
                var alert = new DynamicAlert
                {
                    AlertId = Guid.NewGuid(),
                    AlertType = AlertType.PrinterHealth,
                    Severity = DeterminePrinterAlertSeverity(metrics),
                    Title = $"Alerta de Impresora {metrics.PrinterId}",
                    Description = GeneratePrinterAlertDescription(metrics),
                    Timestamp = DateTime.UtcNow,
                    Source = "PrinterMonitoring",
                    Metrics = metrics,
                    Thresholds = GetPrinterAlertThresholds()
                };

                // Evaluar si requiere acción inmediata
                alert.RequiresImmediateAction = EvaluateImmediateActionRequired(alert);

                // Determinar canales de notificación
                alert.NotificationChannels = DetermineNotificationChannels(alert);

                // Calcular tiempo de resolución estimado
                alert.EstimatedResolutionTime = CalculateResolutionTime(alert);

                _logger.LogInformation("Alerta dinámica de impresora creada: {AlertId} - {Severity}", alert.AlertId, alert.Severity);

                return alert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando alerta dinámica de impresora");
                throw;
            }
        }

        /// <summary>
        /// Configura reglas de alerta dinámicas
        /// </summary>
        public async Task<AlertConfigurationResult> ConfigureDynamicAlertRulesAsync()
        {
            try
            {
                _logger.LogInformation("Configurando reglas de alertas dinámicas");

                var rules = new List<DynamicAlertRule>
                {
                    new()
                    {
                        RuleId = Guid.NewGuid(),
                        Name = "HighFailureProbability",
                        Description = "Alerta cuando probabilidad de fallo > 85%",
                        Condition = "metrics.FailureProbability > 0.85",
                        Severity = AlertSeverity.Critical,
                        Channels = new[] { NotificationChannel.Email, NotificationChannel.Teams, NotificationChannel.Sms },
                        CooldownMinutes = 30,
                        Enabled = true
                    },
                    new()
                    {
                        RuleId = Guid.NewGuid(),
                        Name = "PrinterOffline",
                        Description = "Alerta cuando impresora está offline > 15 minutos",
                        Condition = "metrics.IsOnline == false && metrics.LastSeenMinutes > 15",
                        Severity = AlertSeverity.High,
                        Channels = new[] { NotificationChannel.Email, NotificationChannel.Teams },
                        CooldownMinutes = 15,
                        Enabled = true
                    },
                    new()
                    {
                        RuleId = Guid.NewGuid(),
                        Name = "LowTonerWarning",
                        Description = "Advertencia cuando toner < 10%",
                        Condition = "metrics.TonerLevel < 0.1",
                        Severity = AlertSeverity.Warning,
                        Channels = new[] { NotificationChannel.Email },
                        CooldownMinutes = 60,
                        Enabled = true
                    },
                    new()
                    {
                        RuleId = Guid.NewGuid(),
                        Name = "HighTemperature",
                        Description = "Alerta cuando temperatura > 60°C",
                        Condition = "metrics.Temperature > 60",
                        Severity = AlertSeverity.High,
                        Channels = new[] { NotificationChannel.Email, NotificationChannel.Teams },
                        CooldownMinutes = 10,
                        Enabled = true
                    },
                    new()
                    {
                        RuleId = Guid.NewGuid(),
                        Name = "SecurityBreach",
                        Description = "Alerta de intento de acceso no autorizado",
                        Condition = "eventType == 'SecurityBreach'",
                        Severity = AlertSeverity.Critical,
                        Channels = new[] { NotificationChannel.Email, NotificationChannel.Teams, NotificationChannel.Sms, NotificationChannel.Phone },
                        CooldownMinutes = 5,
                        Enabled = true
                    }
                };

                var config = new AlertConfigurationResult
                {
                    RulesConfigured = rules,
                    DynamicThresholdsEnabled = true,
                    MultiChannelNotifications = true,
                    EscalationPoliciesEnabled = true,
                    AlertRetentionDays = 90,
                    Applied = true,
                    ConfigurationFile = "alert-rules.json"
                };

                _logger.LogInformation("Reglas de alertas dinámicas configuradas: {Count} reglas", rules.Count);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando reglas de alertas dinámicas");
                return new AlertConfigurationResult { Applied = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// Envía notificación de prueba a todos los canales
        /// </summary>
        public async Task<NotificationTestResult> SendTestNotificationAsync()
        {
            try
            {
                _logger.LogInformation("Enviando notificación de prueba a todos los canales");

                var testAlert = new StructuredAlert
                {
                    AlertId = Guid.NewGuid(),
                    AlertType = AlertType.System,
                    Severity = AlertSeverity.Info,
                    Title = "Notificación de Prueba - Sistema de Alertas",
                    Description = "Esta es una notificación de prueba del sistema de alertas avanzadas",
                    Timestamp = DateTime.UtcNow,
                    Source = "AlertSystemTest"
                };

                var channels = Enum.GetValues(typeof(NotificationChannel)).Cast<NotificationChannel>().ToList();
                var results = await SendNotificationsAsync(testAlert, channels);

                var testResult = new NotificationTestResult
                {
                    TestId = Guid.NewGuid(),
                    TestTime = DateTime.UtcNow,
                    ChannelsTested = channels.Count,
                    SuccessfulChannels = results.Count(r => r.Success),
                    FailedChannels = results.Count(r => !r.Success),
                    Results = results,
                    OverallSuccess = results.All(r => r.Success)
                };

                _logger.LogInformation("Notificación de prueba completada: {Successful}/{Total} canales exitosos",
                    testResult.SuccessfulChannels, testResult.ChannelsTested);

                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de prueba");
                return new NotificationTestResult
                {
                    TestId = Guid.NewGuid(),
                    TestTime = DateTime.UtcNow,
                    OverallSuccess = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Obtiene historial de alertas para análisis
        /// </summary>
        public async Task<AlertHistoryReport> GetAlertHistoryAsync(DateTime fromDate, DateTime toDate, AlertSeverity? severity = null)
        {
            try
            {
                var report = new AlertHistoryReport
                {
                    Period = new DateTimeRange(fromDate, toDate),
                    TotalAlerts = 0, // Simulado - en producción vendría de BD
                    AlertsBySeverity = new Dictionary<AlertSeverity, int>(),
                    AlertsByType = new Dictionary<AlertType, int>(),
                    AlertsByChannel = new Dictionary<NotificationChannel, int>(),
                    AverageResponseTime = TimeSpan.FromMinutes(5),
                    FalsePositiveRate = 0.02, // 2% de falsos positivos
                    TopAlertTypes = new List<AlertTypeCount>()
                };

                // Datos simulados para demostración
                report.TotalAlerts = 156;
                report.AlertsBySeverity[AlertSeverity.Critical] = 3;
                report.AlertsBySeverity[AlertSeverity.High] = 12;
                report.AlertsBySeverity[AlertSeverity.Warning] = 45;
                report.AlertsBySeverity[AlertSeverity.Info] = 96;

                report.AlertsByType[AlertType.PrinterHealth] = 89;
                report.AlertsByType[AlertType.Security] = 15;
                report.AlertsByType[AlertType.System] = 32;
                report.AlertsByType[AlertType.Performance] = 20;

                report.AlertsByChannel[NotificationChannel.Email] = 156;
                report.AlertsByChannel[NotificationChannel.Teams] = 45;
                report.AlertsByChannel[NotificationChannel.Sms] = 3;

                report.TopAlertTypes.Add(new AlertTypeCount { Type = AlertType.PrinterHealth, Count = 89 });
                report.TopAlertTypes.Add(new AlertTypeCount { Type = AlertType.System, Count = 32 });
                report.TopAlertTypes.Add(new AlertTypeCount { Type = AlertType.Security, Count = 15 });

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de alertas");
                return new AlertHistoryReport();
            }
        }

        // Métodos auxiliares privados

        private async Task<bool> EvaluateAlertRulesAsync(AlertEvent alertEvent)
        {
            // Lógica simplificada - en producción sería más compleja
            switch (alertEvent.EventType)
            {
                case "HighFailureProbability":
                    return alertEvent.Severity >= AlertSeverity.High;
                case "PrinterOffline":
                    return alertEvent.Severity >= AlertSeverity.Warning;
                case "SecurityBreach":
                    return true; // Siempre alertar eventos de seguridad
                default:
                    return alertEvent.Severity >= AlertSeverity.Warning;
            }
        }

        private StructuredAlert CreateStructuredAlert(AlertEvent alertEvent)
        {
            return new StructuredAlert
            {
                AlertId = Guid.NewGuid(),
                AlertType = MapEventTypeToAlertType(alertEvent.EventType),
                Severity = alertEvent.Severity,
                Title = GenerateAlertTitle(alertEvent),
                Description = alertEvent.Description,
                Timestamp = DateTime.UtcNow,
                Source = alertEvent.Source,
                EventData = alertEvent.EventData,
                RequiresImmediateAction = alertEvent.Severity >= AlertSeverity.Critical
            };
        }

        private List<NotificationChannel> DetermineNotificationChannels(AlertEvent alertEvent)
        {
            var channels = new List<NotificationChannel>();

            switch (alertEvent.Severity)
            {
                case AlertSeverity.Critical:
                    channels.AddRange(new[] { NotificationChannel.Email, NotificationChannel.Teams, NotificationChannel.Sms, NotificationChannel.Phone });
                    break;
                case AlertSeverity.High:
                    channels.AddRange(new[] { NotificationChannel.Email, NotificationChannel.Teams });
                    break;
                case AlertSeverity.Warning:
                    channels.Add(NotificationChannel.Email);
                    break;
                case AlertSeverity.Info:
                    channels.Add(NotificationChannel.Email);
                    break;
            }

            return channels;
        }

        private async Task<List<NotificationResult>> SendNotificationsAsync(StructuredAlert alert, List<NotificationChannel> channels)
        {
            var results = new List<NotificationResult>();

            foreach (var channel in channels)
            {
                try
                {
                    var result = await SendNotificationToChannelAsync(alert, channel);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new NotificationResult
                    {
                        Channel = channel,
                        Success = false,
                        Error = ex.Message,
                        SentAt = DateTime.UtcNow
                    });
                }
            }

            return results;
        }

        private async Task<NotificationResult> SendNotificationToChannelAsync(StructuredAlert alert, NotificationChannel channel)
        {
            // Simulación de envío - en producción implementarías envío real
            await Task.Delay(100); // Simular tiempo de envío

            return new NotificationResult
            {
                Channel = channel,
                Success = true,
                MessageId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                DeliveryConfirmed = true
            };
        }

        private async Task LogAlertEventAsync(StructuredAlert alert, AlertResult result)
        {
            _loggingService.LogApplicationEvent(
                "alert_sent",
                $"Alerta enviada: {alert.Title}",
                MapSeverityToLogLevel(alert.Severity),
                additionalData: new Dictionary<string, object>
                {
                    ["AlertId"] = alert.AlertId,
                    ["AlertType"] = alert.AlertType.ToString(),
                    ["Severity"] = alert.Severity.ToString(),
                    ["Channels"] = string.Join(", ", alert.NotificationChannels.Select(c => c.ToString())),
                    ["RequiresImmediateAction"] = alert.RequiresImmediateAction
                });
        }

        private AlertSeverity DeterminePrinterAlertSeverity(PrinterMetrics metrics)
        {
            if (metrics.FailureProbability > 0.85) return AlertSeverity.Critical;
            if (!metrics.IsOnline || metrics.Temperature > 60) return AlertSeverity.High;
            if (metrics.TonerLevel < 0.1 || metrics.PaperLevel < 0.1) return AlertSeverity.Warning;
            return AlertSeverity.Info;
        }

        private string GeneratePrinterAlertDescription(PrinterMetrics metrics)
        {
            var issues = new List<string>();

            if (metrics.FailureProbability > 0.85)
                issues.Add($"Alta probabilidad de fallo ({metrics.FailureProbability:P1})");
            if (!metrics.IsOnline)
                issues.Add("Impresora offline");
            if (metrics.Temperature > 60)
                issues.Add($"Temperatura alta ({metrics.Temperature}°C)");
            if (metrics.TonerLevel < 0.1)
                issues.Add($"Toner bajo ({metrics.TonerLevel:P0})");
            if (metrics.PaperLevel < 0.1)
                issues.Add($"Papel bajo ({metrics.PaperLevel:P0})");

            return $"Problemas detectados: {string.Join(", ", issues)}";
        }

        private Dictionary<string, object> GetPrinterAlertThresholds()
        {
            return new Dictionary<string, object>
            {
                ["FailureProbability"] = 0.85,
                ["OfflineMinutes"] = 15,
                ["Temperature"] = 60,
                ["TonerLevel"] = 0.1,
                ["PaperLevel"] = 0.1
            };
        }

        private bool EvaluateImmediateActionRequired(DynamicAlert alert)
        {
            return alert.Severity == AlertSeverity.Critical ||
                   alert.AlertType == AlertType.Security ||
                   alert.Metrics != null && (bool)alert.Metrics.GetValueOrDefault("RequiresImmediateAction", false);
        }

        private TimeSpan CalculateResolutionTime(DynamicAlert alert)
        {
            switch (alert.Severity)
            {
                case AlertSeverity.Critical: return TimeSpan.FromMinutes(15);
                case AlertSeverity.High: return TimeSpan.FromMinutes(30);
                case AlertSeverity.Warning: return TimeSpan.FromHours(1);
                default: return TimeSpan.FromHours(2);
            }
        }

        private List<NotificationChannel> DetermineNotificationChannels(DynamicAlert alert)
        {
            return DetermineNotificationChannels(new AlertEvent
            {
                EventType = alert.AlertType.ToString(),
                Severity = alert.Severity,
                Description = alert.Description,
                Source = alert.Source
            });
        }

        // Mapeos auxiliares
        private AlertType MapEventTypeToAlertType(string eventType)
        {
            return eventType switch
            {
                "HighFailureProbability" or "PrinterOffline" => AlertType.PrinterHealth,
                "SecurityBreach" => AlertType.Security,
                "SystemDown" => AlertType.System,
                "PerformanceIssue" => AlertType.Performance,
                _ => AlertType.System
            };
        }

        private string GenerateAlertTitle(AlertEvent alertEvent)
        {
            return $"{alertEvent.Severity} - {alertEvent.EventType}";
        }

        private ApplicationLogLevel MapSeverityToLogLevel(AlertSeverity severity)
        {
            return severity switch
            {
                AlertSeverity.Critical => ApplicationLogLevel.Critical,
                AlertSeverity.High => ApplicationLogLevel.Error,
                AlertSeverity.Warning => ApplicationLogLevel.Warning,
                _ => ApplicationLogLevel.Info
            };
        }
    }

    /// <summary>
    /// DTO para evento de alerta
    /// </summary>
    public class AlertEvent
    {
        public string EventType { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, object>? EventData { get; set; }
    }

    /// <summary>
    /// DTO para resultado de procesamiento de alerta
    /// </summary>
    public class AlertResult
    {
        public Guid EventId { get; set; }
        public DateTime EventReceived { get; set; }
        public string EventType { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string Source { get; set; } = string.Empty;
        public AlertStatus Status { get; set; }
        public bool Processed { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string? Reason { get; set; }
        public string? Error { get; set; }
        public List<NotificationResult> NotificationsSent { get; set; } = new();
    }

    /// <summary>
    /// DTO para alerta estructurada
    /// </summary>
    public class StructuredAlert
    {
        public Guid AlertId { get; set; }
        public AlertType AlertType { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, object>? EventData { get; set; }
        public bool RequiresImmediateAction { get; set; }
        public List<NotificationChannel> NotificationChannels { get; set; } = new();
    }

    /// <summary>
    /// DTO para alerta dinámica de impresora
    /// </summary>
    public class DynamicAlert
    {
        public Guid AlertId { get; set; }
        public AlertType AlertType { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
        public PrinterMetrics? Metrics { get; set; }
        public Dictionary<string, object>? Thresholds { get; set; }
        public bool RequiresImmediateAction { get; set; }
        public List<NotificationChannel> NotificationChannels { get; set; } = new();
        public TimeSpan EstimatedResolutionTime { get; set; }
    }

    /// <summary>
    /// DTO para métricas de impresora
    /// </summary>
    public class PrinterMetrics
    {
        public int PrinterId { get; set; }
        public bool IsOnline { get; set; }
        public double FailureProbability { get; set; }
        public double Temperature { get; set; }
        public double TonerLevel { get; set; }
        public double PaperLevel { get; set; }
        public int LastSeenMinutes { get; set; }
    }

    /// <summary>
    /// DTO para regla de alerta dinámica
    /// </summary>
    public class DynamicAlertRule
    {
        public Guid RuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public NotificationChannel[] Channels { get; set; } = Array.Empty<NotificationChannel>();
        public int CooldownMinutes { get; set; }
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// DTO para resultado de configuración de alertas
    /// </summary>
    public class AlertConfigurationResult
    {
        public List<DynamicAlertRule> RulesConfigured { get; set; } = new();
        public bool DynamicThresholdsEnabled { get; set; }
        public bool MultiChannelNotifications { get; set; }
        public bool EscalationPoliciesEnabled { get; set; }
        public int AlertRetentionDays { get; set; }
        public bool Applied { get; set; }
        public string? Error { get; set; }
        public string ConfigurationFile { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resultado de prueba de notificaciones
    /// </summary>
    public class NotificationTestResult
    {
        public Guid TestId { get; set; }
        public DateTime TestTime { get; set; }
        public int ChannelsTested { get; set; }
        public int SuccessfulChannels { get; set; }
        public int FailedChannels { get; set; }
        public List<NotificationResult> Results { get; set; } = new();
        public bool OverallSuccess { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// DTO para resultado de notificación individual
    /// </summary>
    public class NotificationResult
    {
        public NotificationChannel Channel { get; set; }
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public DateTime SentAt { get; set; }
        public bool DeliveryConfirmed { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// DTO para reporte de historial de alertas
    /// </summary>
    public class AlertHistoryReport
    {
        public DateTimeRange Period { get; set; } = new();
        public int TotalAlerts { get; set; }
        public Dictionary<AlertSeverity, int> AlertsBySeverity { get; set; } = new();
        public Dictionary<AlertType, int> AlertsByType { get; set; } = new();
        public Dictionary<NotificationChannel, int> AlertsByChannel { get; set; } = new();
        public TimeSpan AverageResponseTime { get; set; }
        public double FalsePositiveRate { get; set; }
        public List<AlertTypeCount> TopAlertTypes { get; set; } = new();
    }

    /// <summary>
    /// DTO auxiliar para conteo de tipos de alerta
    /// </summary>
    public class AlertTypeCount
    {
        public AlertType Type { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Enumeraciones para tipos de alerta
    /// </summary>
    public enum AlertSeverity { Info, Warning, High, Critical }
    public enum AlertType { PrinterHealth, Security, System, Performance, Network }
    public enum AlertStatus { Skipped, Sent, Failed, Escalated }
    public enum NotificationChannel { Email, Teams, Slack, Sms, Phone, Webhook }
}
