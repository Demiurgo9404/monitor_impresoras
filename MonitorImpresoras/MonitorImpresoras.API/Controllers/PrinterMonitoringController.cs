using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Services.SNMP;

namespace MonitorImpresoras.API.Controllers
{
    /// <summary>
    /// Controlador para monitoreo de impresoras
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PrinterMonitoringController : ControllerBase
    {
        private readonly IPrinterMonitoringService _monitoringService;
        private readonly ISnmpService _snmpService;

        public PrinterMonitoringController(
            IPrinterMonitoringService monitoringService,
            ISnmpService snmpService)
        {
            _monitoringService = monitoringService;
            _snmpService = snmpService;
        }

        /// <summary>
        /// Monitorea todas las impresoras
        /// </summary>
        [HttpPost("monitor-all")]
        public async Task<IActionResult> MonitorAllPrinters()
        {
            try
            {
                var monitoredCount = await _monitoringService.MonitorAllPrintersAsync();
                return Ok(new { MonitoredPrinters = monitoredCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el estado de una impresora específica
        /// </summary>
        /// <param name="ipAddress">Dirección IP de la impresora</param>
        [HttpGet("status/{ipAddress}")]
        public async Task<IActionResult> GetPrinterStatus(string ipAddress)
        {
            try
            {
                var status = await _snmpService.GetPrinterStatusAsync(ipAddress);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de monitoreo
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetMonitoringStats()
        {
            try
            {
                var stats = await _monitoringService.GetMonitoringStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
