using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Technician")]
    public class MonitoringController : ControllerBase
    {
        private readonly IPrinterMonitoringService _monitoringService;
        private readonly IPrinterRepository _printerRepository;
        private readonly ILogger<MonitoringController> _logger;

        public MonitoringController(
            IPrinterMonitoringService monitoringService,
            IPrinterRepository printerRepository,
            ILogger<MonitoringController> logger)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _printerRepository = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene el estado de todas las impresoras
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetAllPrintersStatus()
        {
            try
            {
                await _monitoringService.UpdateAllPrintersStatusAsync();
                var printers = await _printerRepository.GetAllAsync();
                return Ok(printers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de las impresoras");
                return StatusCode(500, "Error interno del servidor al obtener el estado de las impresoras");
            }
        }

        /// <summary>
        /// Obtiene el estado de una impresora específica
        /// </summary>
        /// <param name="id">ID de la impresora</param>
        [HttpGet("printers/{id}/status")]
        public async Task<IActionResult> GetPrinterStatus(int id)
        {
            try
            {
                var printer = await _printerRepository.GetByIdAsync(id);
                if (printer == null)
                {
                    return NotFound($"No se encontró la impresora con ID {id}");
                }

                var status = await _monitoringService.GetPrinterStatusAsync(printer);
                return Ok(new { printer.Id, printer.Name, Status = status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la impresora {PrinterId}", id);
                return StatusCode(500, $"Error interno del servidor al obtener el estado de la impresora {id}");
            }
        }

        /// <summary>
        /// Actualiza el estado de todas las impresoras
        /// </summary>
        [HttpPost("refresh-all")]
        public async Task<IActionResult> RefreshAllPrinters()
        {
            try
            {
                await _monitoringService.UpdateAllPrintersStatusAsync();
                return Ok("Estado de todas las impresoras actualizado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado de las impresoras");
                return StatusCode(500, "Error interno del servidor al actualizar el estado de las impresoras");
            }
        }
    }
}
