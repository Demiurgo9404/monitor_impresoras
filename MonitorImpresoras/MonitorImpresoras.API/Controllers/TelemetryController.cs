using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(ITelemetryService telemetryService, ILogger<TelemetryController> logger)
        {
            _telemetryService = telemetryService;
            _logger = logger;
        }

        [HttpGet("printer/{printerId}")]
        public async Task<ActionResult<PrinterTelemetry>> GetLatestTelemetry(int printerId)
        {
            var telemetry = await _telemetryService.GetLatestTelemetryAsync(printerId);
            if (telemetry == null) return NotFound();
            return Ok(telemetry);
        }

        [HttpGet("printer/{printerId}/history")]
        public async Task<ActionResult<List<PrinterTelemetry>>> GetHistoricalData(
            int printerId, 
            [FromQuery] DateTime from, 
            [FromQuery] DateTime? to = null)
        {
            var endDate = to ?? DateTime.UtcNow;
            var data = await _telemetryService.GetHistoricalDataAsync(printerId, from, endDate);
            return Ok(data);
        }

        [HttpPost("collect")]
        public async Task<IActionResult> CollectTelemetry([FromQuery] int printerId)
        {
            try
            {
                // Aquí se implementaría la lógica para recolectar datos reales de la impresora
                // Por ahora simulamos datos de telemetría
                var telemetry = new PrinterTelemetry
                {
                    PrinterId = printerId,
                    TimestampUtc = DateTime.UtcNow,
                    PagesPrinted = new Random().Next(0, 1000),
                    TonerLevel = new Random().Next(0, 100),
                    PaperLevel = new Random().Next(0, 100),
                    ErrorsCount = new Random().Next(0, 5),
                    Status = "Online",
                    Temperature = new Random().Next(20, 35),
                    IsOnline = true
                };

                var result = await _telemetryService.SaveTelemetryAsync(telemetry);

                if (result)
                {
                    _logger.LogInformation("Telemetría recolectada manualmente para impresora {PrinterId}", printerId);
                    return Ok(new { Success = true, Message = "Telemetría recolectada exitosamente" });
                }

                return BadRequest(new { Success = false, Message = "Error al guardar telemetría" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recolectando telemetría para impresora {PrinterId}", printerId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}
