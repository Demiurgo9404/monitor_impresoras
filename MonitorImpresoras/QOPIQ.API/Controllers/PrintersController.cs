using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOPIQ.Application.Services;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QOPIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintersController : ControllerBase
    {
        private readonly IPrinterService _printerService;
        private readonly ILogger<PrintersController> _logger;

        public PrintersController(
            IPrinterService printerService,
            ILogger<PrintersController> logger)
        {
            _printerService = printerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Printer>>> GetAll()
        {
            try
            {
                var printers = await _printerService.GetAllPrintersAsync();
                return Ok(printers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las impresoras");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Printer>> GetById(Guid id)
        {
            try
            {
                var printer = await _printerService.GetPrinterByIdAsync(id);
                if (printer == null)
                    return NotFound();

                return Ok(printer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la impresora con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Printer>> Create(PrinterCreateDto printerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var printer = new Printer
                {
                    Name = printerDto.Name,
                    Model = printerDto.Model,
                    IpAddress = printerDto.IpAddress,
                    Location = printerDto.Location,
                    IsActive = true,
                    Created = DateTime.UtcNow
                };

                await _printerService.AddPrinterAsync(printer);
                
                return CreatedAtAction(nameof(GetById), new { id = printer.Id }, printer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la impresora");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, PrinterUpdateDto printerDto)
        {
            try
            {
                if (id != printerDto.Id)
                    return BadRequest("ID de la URL no coincide con el ID del cuerpo de la solicitud");

                var existingPrinter = await _printerService.GetPrinterByIdAsync(id);
                if (existingPrinter == null)
                    return NotFound();

                existingPrinter.Name = printerDto.Name;
                existingPrinter.Model = printerDto.Model;
                existingPrinter.IpAddress = printerDto.IpAddress;
                existingPrinter.Location = printerDto.Location;
                existingPrinter.IsActive = printerDto.IsActive;
                existingPrinter.LastUpdated = DateTime.UtcNow;

                await _printerService.UpdatePrinterAsync(existingPrinter);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar la impresora con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var printer = await _printerService.GetPrinterByIdAsync(id);
                if (printer == null)
                    return NotFound();

                await _printerService.DeletePrinterAsync(id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar la impresora con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}/status")]
        public async Task<ActionResult<PrinterStatusDto>> GetStatus(Guid id)
        {
            try
            {
                var status = await _printerService.GetPrinterStatusAsync(id);
                if (status == null)
                    return NotFound();

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el estado de la impresora con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
