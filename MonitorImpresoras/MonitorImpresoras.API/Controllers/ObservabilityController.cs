using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ObservabilityController : ControllerBase
    {
        private readonly ICentralizedLoggingService _loggingService;
        private readonly IComprehensiveMetricsService _metricsService;
        private readonly IExtendedHealthCheckService _healthService;
        private readonly IAdvancedAlertService _alertService;
        private readonly ILogger<ObservabilityController> _logger;

        public ObservabilityController(
            ICentralizedLoggingService loggingService,
            IComprehensiveMetricsService metricsService,
            IExtendedHealthCheckService healthService,
            IAdvancedAlertService alertService,
            ILogger<ObservabilityController> logger)
        {
            _loggingService = loggingService;
            _metricsService = metricsService;
            _healthService = healthService;
            _alertService = alertService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene métricas actuales del sistema
        /// </summary>
        [HttpGet("metrics/current")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCurrentSystemMetrics()
        {
            try
            {
                _logger.LogInformation("Obteniendo métricas actuales del sistema por usuario {UserId}", User.Identity?.Name);

                var metrics = await _metricsService.GetCurrentSystemMetricsAsync();

                return Ok(new
                {
                    Message = "Métricas actuales del sistema",
                    CurrentMetrics = metrics,
                    GeneratedBy = User.Identity?.Name,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas actuales del sistema");
                throw;
            }
        }

        /// <summary>
        /// Obtiene métricas históricas para análisis
        /// </summary>
        [HttpGet("metrics/history")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHistoricalMetrics([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                _logger.LogInformation("Obteniendo métricas históricas por usuario {UserId}", User.Identity?.Name);

                var report = await _metricsService.GetHistoricalMetricsAsync(fromDate, toDate);

                return Ok(new
                {
                    Message = "Reporte de métricas históricas",
                    HistoricalReport = report,
                    GeneratedBy = User.Identity?.Name,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas históricas");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta health checks extendidos del sistema
        /// </summary>
        [HttpPost("health/extended")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RunExtendedHealthChecks()
        {
            try
            {
                _logger.LogInformation("Ejecutando health checks extendidos por usuario {UserId}", User.Identity?.Name);

                var report = await _healthService.RunExtendedHealthChecksAsync();

                return Ok(new
                {
                    Message = "Health checks extendidos completados",
                    HealthReport = report,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando health checks extendidos");
                throw;
            }
        }

        /// <summary>
        /// Registra evento personalizado en el sistema de logging
        /// </summary>
        [HttpPost("log/event")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult LogCustomEvent([FromBody] CustomLogEvent logEvent)
        {
            try
            {
                _logger.LogInformation("Registrando evento personalizado por usuario {UserId}", User.Identity?.Name);

                _loggingService.LogApplicationEvent(
                    logEvent.EventType,
                    logEvent.Message,
                    logEvent.Level,
                    User.Identity?.Name,
                    additionalData: logEvent.AdditionalData);

                return Ok(new
                {
                    Message = "Evento registrado exitosamente",
                    EventId = Guid.NewGuid(),
                    LoggedBy = User.Identity?.Name,
                    LoggedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando evento personalizado");
                throw;
            }
        }

        /// <summary>
        /// Crea alerta dinámica basada en métricas de impresora
        /// </summary>
        [HttpPost("alerts/printer")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreatePrinterAlert([FromBody] PrinterMetrics metrics)
        {
            try
            {
                _logger.LogInformation("Creando alerta dinámica de impresora por usuario {UserId}", User.Identity?.Name);

                var alert = await _alertService.CreateDynamicPrinterAlertAsync(metrics);

                return Ok(new
                {
                    Message = "Alerta de impresora creada",
                    PrinterAlert = alert,
                    CreatedBy = User.Identity?.Name,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando alerta de impresora");
                throw;
            }
        }

        /// <summary>
        /// Procesa evento de alerta personalizado
        /// </summary>
        [HttpPost("alerts/process")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ProcessAlertEvent([FromBody] AlertEvent alertEvent)
        {
            try
            {
                _logger.LogInformation("Procesando evento de alerta por usuario {UserId}", User.Identity?.Name);

                var result = await _alertService.ProcessAlertEventAsync(alertEvent);

                return Ok(new
                {
                    Message = "Evento de alerta procesado",
                    AlertResult = result,
                    ProcessedBy = User.Identity?.Name,
                    ProcessedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento de alerta");
                throw;
            }
        }

        /// <summary>
        /// Obtiene historial de alertas
        /// </summary>
        [HttpGet("alerts/history")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAlertHistory([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string? severity = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo historial de alertas por usuario {UserId}", User.Identity?.Name);

                AlertSeverity? severityFilter = null;
                if (!string.IsNullOrEmpty(severity) && Enum.TryParse<AlertSeverity>(severity, true, out var parsedSeverity))
                {
                    severityFilter = parsedSeverity;
                }

                var report = await _alertService.GetAlertHistoryAsync(fromDate, toDate, severityFilter);

                return Ok(new
                {
                    Message = "Reporte de historial de alertas",
                    AlertHistory = report,
                    GeneratedBy = User.Identity?.Name,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de alertas");
                throw;
            }
        }

        /// <summary>
        /// Configura reglas de alertas dinámicas
        /// </summary>
        [HttpPost("alerts/configure")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ConfigureAlertRules()
        {
            try
            {
                _logger.LogInformation("Configurando reglas de alertas dinámicas por usuario {UserId}", User.Identity?.Name);

                var result = await _alertService.ConfigureDynamicAlertRulesAsync();

                return Ok(new
                {
                    Message = "Reglas de alertas configuradas",
                    ConfigurationResult = result,
                    ConfiguredBy = User.Identity?.Name,
                    ConfiguredAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando reglas de alertas");
                throw;
            }
        }

        /// <summary>
        /// Envía notificación de prueba a todos los canales
        /// </summary>
        [HttpPost("alerts/test")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SendTestNotification()
        {
            try
            {
                _logger.LogInformation("Enviando notificación de prueba por usuario {UserId}", User.Identity?.Name);

                var result = await _alertService.SendTestNotificationAsync();

                return Ok(new
                {
                    Message = "Notificación de prueba enviada",
                    TestResult = result,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de prueba");
                throw;
            }
        }

        /// <summary>
        /// Configura logging estructurado para toda la aplicación
        /// </summary>
        [HttpPost("logging/configure")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ConfigureStructuredLogging()
        {
            try
            {
                _logger.LogInformation("Configurando logging estructurado por usuario {UserId}", User.Identity?.Name);

                var result = await _loggingService.ConfigureStructuredLoggingAsync();

                return Ok(new
                {
                    Message = "Logging estructurado configurado",
                    ConfigurationResult = result,
                    ConfiguredBy = User.Identity?.Name,
                    ConfiguredAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando logging estructurado");
                throw;
            }
        }

        /// <summary>
        /// Registra métricas de operación HTTP manualmente
        /// </summary>
        [HttpPost("metrics/http")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult RecordHttpMetrics([FromBody] HttpMetricsRequest request)
        {
            try
            {
                _logger.LogInformation("Registrando métricas HTTP manualmente por usuario {UserId}", User.Identity?.Name);

                _metricsService.RecordHttpOperation(
                    request.Method,
                    request.Endpoint,
                    request.StatusCode,
                    request.Duration,
                    User.Identity?.Name,
                    request.RequestSize,
                    request.ResponseSize);

                return Ok(new
                {
                    Message = "Métricas HTTP registradas",
                    RecordedBy = User.Identity?.Name,
                    RecordedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métricas HTTP");
                throw;
            }
        }

        /// <summary>
        /// Registra métricas de operación de base de datos manualmente
        /// </summary>
        [HttpPost("metrics/database")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult RecordDatabaseMetrics([FromBody] DatabaseMetricsRequest request)
        {
            try
            {
                _logger.LogInformation("Registrando métricas de BD manualmente por usuario {UserId}", User.Identity?.Name);

                _metricsService.RecordDatabaseOperation(
                    request.Operation,
                    request.TableName,
                    request.OperationType,
                    request.Duration,
                    request.RowsAffected,
                    request.Success,
                    User.Identity?.Name);

                return Ok(new
                {
                    Message = "Métricas de BD registradas",
                    RecordedBy = User.Identity?.Name,
                    RecordedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métricas de BD");
                throw;
            }
        }

        /// <summary>
        /// Registra métricas de operación de IA manualmente
        /// </summary>
        [HttpPost("metrics/ai")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult RecordAiMetrics([FromBody] AiMetricsRequest request)
        {
            try
            {
                _logger.LogInformation("Registrando métricas de IA manualmente por usuario {UserId}", User.Identity?.Name);

                _metricsService.RecordAiOperation(
                    request.ModelType,
                    request.Operation,
                    request.Result,
                    request.Duration,
                    request.Metrics,
                    User.Identity?.Name);

                return Ok(new
                {
                    Message = "Métricas de IA registradas",
                    RecordedBy = User.Identity?.Name,
                    RecordedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métricas de IA");
                throw;
            }
        }

        /// <summary>
        /// Obtiene métricas de rendimiento en tiempo real para dashboard
        /// </summary>
        [HttpGet("dashboard/realtime")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRealtimeDashboardMetrics()
        {
            try
            {
                _logger.LogInformation("Obteniendo métricas de dashboard en tiempo real por usuario {UserId}", User.Identity?.Name);

                var systemMetrics = await _metricsService.GetCurrentSystemMetricsAsync();
                var healthReport = await _healthService.RunExtendedHealthChecksAsync();

                var dashboardData = new
                {
                    Timestamp = DateTime.UtcNow,
                    SystemMetrics = systemMetrics,
                    HealthStatus = healthReport.OverallStatus,
                    HealthyChecks = healthReport.GetHealthyChecksCount(),
                    TotalChecks = healthReport.GetTotalChecksCount(),
                    RecentAlerts = await _alertService.GetAlertHistoryAsync(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow),
                    GeneratedBy = User.Identity?.Name,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(new
                {
                    Message = "Métricas de dashboard en tiempo real",
                    DashboardData = dashboardData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas de dashboard");
                throw;
            }
        }
    }

    /// <summary>
    /// DTO para evento de log personalizado
    /// </summary>
    public class CustomLogEvent
    {
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ApplicationLogLevel Level { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// DTO para request de métricas HTTP
    /// </summary>
    public class HttpMetricsRequest
    {
        public string Method { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public TimeSpan Duration { get; set; }
        public long? RequestSize { get; set; }
        public long? ResponseSize { get; set; }
    }

    /// <summary>
    /// DTO para request de métricas de base de datos
    /// </summary>
    public class DatabaseMetricsRequest
    {
        public string Operation { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public DatabaseOperationType OperationType { get; set; }
        public TimeSpan Duration { get; set; }
        public int RowsAffected { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// DTO para request de métricas de IA
    /// </summary>
    public class AiMetricsRequest
    {
        public string ModelType { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public AiOperationResult Result { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, object>? Metrics { get; set; }
    }
}
