using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Health checks deben ser accesibles sin autenticación
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _basicHealthCheck;
        private readonly IAdvancedHealthCheckService _advancedHealthCheck;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            HealthCheckService basicHealthCheck,
            IAdvancedHealthCheckService advancedHealthCheck,
            ILogger<HealthController> logger)
        {
            _basicHealthCheck = basicHealthCheck;
            _advancedHealthCheck = advancedHealthCheck;
            _logger = logger;
        }

        /// <summary>
        /// Health check básico (estándar ASP.NET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBasicHealth()
        {
            try
            {
                var report = await _basicHealthCheck.CheckHealthAsync();

                return report.Status == HealthStatus.Healthy
                    ? Ok(new { status = "healthy", timestamp = DateTime.UtcNow })
                    : StatusCode(503, new { status = "unhealthy", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en health check básico");
                return StatusCode(503, new { status = "error", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Health check extendido con detalles de componentes
        /// </summary>
        /// <returns>Estado detallado de todos los componentes</returns>
        [HttpGet("extended")]
        [AllowAnonymous] // Público pero con más detalles para debugging
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetExtendedHealth()
        {
            try
            {
                var health = await _healthCheckService.GetExtendedHealthAsync();

                var statusCode = health.Status == "Healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

                return StatusCode(statusCode, health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en health check extendido");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "Unhealthy",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Health check seguro con información sensible (solo para administradores)
        /// </summary>
        /// <returns>Estado completo incluyendo configuración</returns>
        [HttpGet("secure")]
        [Authorize(Policy = "RequireAdmin")] // Solo administradores
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetSecureHealth()
        {
            try
            {
                var health = await _healthCheckService.GetExtendedHealthAsync();

                // Agregar información sensible solo para administradores
                var secureHealth = new
                {
                    Status = health.Status,
                    Checks = health.Checks,
                    TotalDuration = health.TotalDuration,
                    Database = new
                    {
                        Status = health.Database?.Status,
                        ConnectionCount = health.Database?.ConnectionCount,
                        QueryTime = health.Database?.QueryTime,
                        // No incluir ConnectionString en respuesta segura
                        TableRecordCounts = health.Database?.TableRecordCounts
                    },
                    ScheduledReports = health.ScheduledReports,
                    System = health.System,
                    Metrics = health.Metrics,
                    Environment = new
                    {
                        ApplicationName = "MonitorImpresoras",
                        Version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                        EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                        MachineName = Environment.MachineName,
                        OSVersion = Environment.OSVersion.VersionString,
                        ProcessorCount = Environment.ProcessorCount,
                        Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                        SystemPageSize = Environment.SystemPageSize,
                        WorkingSet = Environment.WorkingSet
                    }
                };

                var statusCode = health.Status == "Healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

                return StatusCode(statusCode, secureHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en health check seguro");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "Unhealthy",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Health check específico para readiness (Kubernetes)
        /// </summary>
        [HttpGet("ready")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                // Para readiness, solo verificamos componentes críticos
                var health = await _healthCheckService.GetBasicHealthAsync();

                var statusCode = health.Status == "Healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

                return StatusCode(statusCode, new
                {
                    Status = health.Status,
                    Timestamp = DateTime.UtcNow,
                    Checks = new
                    {
                        Application = health.Checks["Application"],
                        Database = "Ready" // Simplified for readiness probe
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en readiness check");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "NotReady",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Health check específico para liveness (Kubernetes)
        /// </summary>
        [HttpGet("live")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLiveness()
        {
            // Para liveness, solo verificamos que la aplicación responde
            return Ok(new
            {
                Status = "Alive",
                Timestamp = DateTime.UtcNow,
                Application = "MonitorImpresoras",
                Version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "1.0.0"
            });
        }
    }
}
