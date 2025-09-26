using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace MonitorImpresoras.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServicesController : ControllerBase
    {
        private readonly IAlertEngineService _alertEngineService;
        private readonly IAlertService _alertService;
        private readonly IPrinterService _printerService;
        private readonly IConsumableService _consumableService;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(
            IAlertEngineService alertEngineService,
            IAlertService alertService,
            IPrinterService printerService,
            IConsumableService consumableService,
            ILogger<ServicesController> logger)
        {
            _alertEngineService = alertEngineService;
            _alertService = alertService;
            _printerService = printerService;
            _consumableService = consumableService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de todas las impresoras registradas en el sistema
        /// </summary>
        /// <returns>Lista de nombres de impresoras</returns>
        /// <response code="200">Lista de impresoras obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("printers")]
        [SwaggerOperation(Summary = "Obtener lista de impresoras", Description = "Retorna los nombres de todas las impresoras registradas en el sistema")]
        [SwaggerResponse(200, "Lista de impresoras obtenida exitosamente", typeof(IEnumerable<string>))]
        [SwaggerResponse(500, "Error interno del servidor")]
        public async Task<IActionResult> GetPrinters()
        {
            try
            {
                var printers = await _printerService.GetPrintersAsync();
                return Ok(printers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printers");
                return StatusCode(500, new { message = "Error retrieving printers" });
            }
        }

        /// <summary>
        /// Obtiene el estado actual de una impresora específica
        /// </summary>
        /// <param name="printerId">ID único de la impresora (GUID)</param>
        /// <returns>Estado de la impresora (Online/Offline)</returns>
        /// <response code="200">Estado de la impresora obtenido exitosamente</response>
        /// <response code="404">Impresora no encontrada</response>
        /// <response code="400">ID de impresora inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("printers/{printerId}/status")]
        [SwaggerOperation(Summary = "Obtener estado de una impresora", Description = "Retorna el estado actual de una impresora específica")]
        [SwaggerResponse(200, "Estado de la impresora obtenido exitosamente", typeof(object))]
        [SwaggerResponse(404, "Impresora no encontrada")]
        [SwaggerResponse(400, "ID de impresora inválido")]
        [SwaggerResponse(500, "Error interno del servidor")]
        public async Task<IActionResult> GetPrinterStatus(Guid printerId)
        {
            try
            {
                var status = await _printerService.GetPrinterStatusAsync(printerId);
                return Ok(new { printerId, status });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer status for {PrinterId}", printerId);
                return StatusCode(500, new { message = "Error retrieving printer status" });
            }
        }

        /// <summary>
        /// Envía una alerta de prueba al sistema
        /// </summary>
        /// <param name="request">Datos de la alerta de prueba</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Alerta enviada exitosamente</response>
        /// <response code="400">Datos de solicitud inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("alerts/test")]
        [SwaggerOperation(Summary = "Enviar alerta de prueba", Description = "Crea y envía una alerta de prueba al sistema")]
        [SwaggerResponse(200, "Alerta enviada exitosamente")]
        [SwaggerResponse(400, "Datos de solicitud inválidos")]
        [SwaggerResponse(500, "Error interno del servidor")]
        public async Task<IActionResult> TestAlert([FromBody] TestAlertRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest(new { message = "Message is required" });
                }

                await _alertService.SendAlertAsync(request.Message, request.Severity ?? "Info");
                return Ok(new { message = "Alert sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test alert");
                return StatusCode(500, new { message = "Error sending alert" });
            }
        }

        /// <summary>
        /// Verifica los niveles de consumibles de una impresora específica
        /// </summary>
        /// <param name="printerId">ID único de la impresora (GUID)</param>
        /// <returns>Resultado de la verificación</returns>
        /// <response code="200">Verificación completada exitosamente</response>
        /// <response code="404">Impresora no encontrada</response>
        /// <response code="400">ID de impresora inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("consumables/check/{printerId}")]
        [SwaggerOperation(Summary = "Verificar consumibles de una impresora", Description = "Analiza los niveles de consumibles de una impresora específica y genera alertas si es necesario")]
        [SwaggerResponse(200, "Verificación completada exitosamente")]
        [SwaggerResponse(404, "Impresora no encontrada")]
        [SwaggerResponse(400, "ID de impresora inválido")]
        [SwaggerResponse(500, "Error interno del servidor")]
        public async Task<IActionResult> CheckConsumables(Guid printerId)
        {
            try
            {
                await _consumableService.CheckConsumablesAsync(printerId);
                return Ok(new { message = $"Consumables check completed for printer {printerId}" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking consumables for printer {PrinterId}", printerId);
                return StatusCode(500, new { message = "Error checking consumables" });
            }
        }

        /// <summary>
        /// Ejecuta el procesamiento de todas las reglas de alertas del sistema
        /// </summary>
        /// <returns>Resultado del procesamiento</returns>
        /// <response code="200">Procesamiento completado exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("alerts/process")]
        [SwaggerOperation(Summary = "Procesar alertas del sistema", Description = "Ejecuta todas las reglas de alertas activas del sistema (impresoras offline, consumibles bajos, errores)")]
        [SwaggerResponse(200, "Procesamiento completado exitosamente")]
        [SwaggerResponse(500, "Error interno del servidor")]
        public async Task<IActionResult> ProcessAlerts()
        {
            try
            {
                await _alertEngineService.ProcessAlertsAsync();
                return Ok(new { message = "Alert processing completed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing alerts");
                return StatusCode(500, new { message = "Error processing alerts" });
            }
        }

        /// <summary>
        /// Endpoint de health check para monitoreo del sistema
        /// </summary>
        /// <returns>Estado de salud del sistema</returns>
        /// <response code="200">Sistema funcionando correctamente</response>
        /// <response code="503">Sistema con problemas</response>
        [HttpGet("health")]
        [AllowAnonymous] // Health check debe ser accesible sin autenticación
        [SwaggerOperation(Summary = "Health Check del sistema", Description = "Verifica el estado de salud de todos los servicios del sistema")]
        [SwaggerResponse(200, "Sistema funcionando correctamente")]
        [SwaggerResponse(503, "Sistema con problemas")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var healthStatus = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    services = new
                    {
                        printerService = await CheckPrinterServiceHealth(),
                        consumableService = await CheckConsumableServiceHealth(),
                        alertService = await CheckAlertServiceHealth(),
                        alertEngineService = await CheckAlertEngineServiceHealth()
                    }
                };

                // Verificar si todos los servicios están saludables
                var allServicesHealthy = healthStatus.services.printerService.healthy &&
                                       healthStatus.services.consumableService.healthy &&
                                       healthStatus.services.alertService.healthy &&
                                       healthStatus.services.alertEngineService.healthy;

                if (!allServicesHealthy)
                {
                    healthStatus = healthStatus with { status = "degraded" };
                    return StatusCode(503, healthStatus);
                }

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = "Health check failed"
                });
            }
        }

        /// <summary>
        /// Obtiene estadísticas generales del sistema
        /// </summary>
        /// <returns>Estadísticas del sistema</returns>
        /// <response code="200">Estadísticas obtenidas exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("stats")]
        [SwaggerOperation(Summary = "Obtener estadísticas del sistema", Description = "Retorna estadísticas generales de todos los servicios")]
        [SwaggerResponse(200, "Estadísticas obtenidas exitosamente")]
        [SwaggerResponse(500, "Error interno del servidor")]
        public async Task<IActionResult> GetSystemStats()
        {
            try
            {
                var stats = new
                {
                    timestamp = DateTime.UtcNow,
                    services = new
                    {
                        printers = new
                        {
                            total = await _printerService.GetPrintersAsync().ContinueWith(t => t.Result.Count()),
                            online = 0, // Podría implementarse lógica para contar impresoras online
                            offline = 0  // Podría implementarse lógica para contar impresoras offline
                        },
                        alerts = new
                        {
                            total = 0, // Podría implementarse lógica para contar alertas totales
                            active = 0, // Podría implementarse lógica para contar alertas activas
                            resolved = 0 // Podría implementarse lógica para contar alertas resueltas
                        },
                        consumables = new
                        {
                            total = 0, // Podría implementarse lógica para contar consumibles totales
                            low = 0,   // Podría implementarse lógica para contar consumibles bajos
                            critical = 0 // Podría implementarse lógica para contar consumibles críticos
                        }
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system stats");
                return StatusCode(500, new { message = "Error retrieving system statistics" });
            }
        }

        // Métodos auxiliares para health check
        private async Task<(bool healthy, string message)> CheckPrinterServiceHealth()
        {
            try
            {
                var printers = await _printerService.GetPrintersAsync();
                return (true, $"Found {printers.Count()} printers");
            }
            catch (Exception ex)
            {
                return (false, $"Printer service error: {ex.Message}");
            }
        }

        private async Task<(bool healthy, string message)> CheckConsumableServiceHealth()
        {
            try
            {
                // Crear un GUID de prueba válido
                var testPrinterId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
                await _consumableService.CheckConsumablesAsync(testPrinterId);
                return (true, "Consumable service operational");
            }
            catch (Exception ex)
            {
                return (false, $"Consumable service error: {ex.Message}");
            }
        }

        private async Task<(bool healthy, string message)> CheckAlertServiceHealth()
        {
            try
            {
                await _alertService.SendAlertAsync("Health check test", "Low");
                return (true, "Alert service operational");
            }
            catch (Exception ex)
            {
                return (false, $"Alert service error: {ex.Message}");
            }
        }

        private async Task<(bool healthy, string message)> CheckAlertEngineServiceHealth()
        {
            try
            {
                await _alertEngineService.ProcessAlertsAsync();
                return (true, "Alert engine service operational");
            }
            catch (Exception ex)
            {
                return (false, $"Alert engine service error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Modelo para solicitud de alerta de prueba
    /// </summary>
    public class TestAlertRequest
    {
        /// <summary>
        /// Mensaje de la alerta
        /// </summary>
        /// <example>La impresora HP LaserJet está experimentando problemas de conexión</example>
        public string Message { get; set; }

        /// <summary>
        /// Nivel de severidad de la alerta
        /// </summary>
        /// <example>High</example>
        public string Severity { get; set; }
    }
}
