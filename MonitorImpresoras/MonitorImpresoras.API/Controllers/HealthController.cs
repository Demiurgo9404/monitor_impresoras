using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Verifica el estado de salud de la API
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var response = new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    checks = new[]
                    {
                        new
                        {
                            name = "API",
                            status = "Healthy",
                            description = "La API está en funcionamiento"
                        }
                    }
                };
                
                _logger.LogInformation("Solicitud de verificación de salud exitosa");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de salud de la API");
                
                var errorResponse = new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    error = new
                    {
                        message = "Error al verificar el estado de salud de la API",
                        details = ex.Message
                    }
                };
                
                return StatusCode(500, errorResponse);
            }
        }
    }
}
