using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.DTOs;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Interfaces.Services;

namespace QOPIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintersController : ControllerBase
    {
        private readonly IPrinterService _printerService;
        private readonly ILogger<PrintersController> _logger;

        public PrintersController(IPrinterService printerService, ILogger<PrintersController> logger)
        {
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrinterDto>>> GetAll()
        {
            try
            {
                var printers = await _printerService.GetAllPrintersAsync();
                var result = printers.Select(p => new PrinterDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    IpAddress = p.IpAddress,
                    Location = p.Location,
                    Model = p.Model,
                    Status = p.Status,
                    StatusMessage = p.StatusMessage,
                    LastStatusUpdate = p.LastStatusUpdate,
                    IsOnline = p.IsOnline,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las impresoras");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PrinterDto>> GetById(Guid id)
        {
            try
            {
                var printer = await _printerService.GetPrinterByIdAsync(id);
                if (printer == null)
                    return NotFound();

                var result = new PrinterDto
                {
                    Id = printer.Id,
                    Name = printer.Name,
                    IpAddress = printer.IpAddress,
                    Location = printer.Location,
                    Model = printer.Model,
                    Status = printer.Status,
                    StatusMessage = printer.StatusMessage,
                    LastStatusUpdate = printer.LastStatusUpdate,
                    IsOnline = printer.IsOnline,
                    IsActive = printer.IsActive,
                    CreatedAt = printer.CreatedAt,
                    UpdatedAt = printer.UpdatedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la impresora con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PrinterDto>> Create(PrinterCreateDto printerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var printer = new Printer
                {
                    Id = Guid.NewGuid(),
                    Name = printerDto.Name,
                    IpAddress = printerDto.IpAddress,
                    Location = printerDto.Location,
                    Model = printerDto.Model,
                    Status = PrinterStatus.Offline,
                    StatusMessage = $"Impresora {printerDto.Name} creada",
                    LastStatusUpdate = DateTime.UtcNow,
                    IsOnline = false,
                    IsActive = printerDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPrinter = await _printerService.CreatePrinterAsync(printer);
                
                var result = new PrinterDto
                {
                    Id = createdPrinter.Id,
                    Name = createdPrinter.Name,
                    IpAddress = createdPrinter.IpAddress,
                    Location = createdPrinter.Location,
                    Model = createdPrinter.Model,
                    Status = createdPrinter.Status,
                    StatusMessage = createdPrinter.StatusMessage,
                    LastStatusUpdate = createdPrinter.LastStatusUpdate,
                    IsOnline = createdPrinter.IsOnline,
                    IsActive = createdPrinter.IsActive,
                    CreatedAt = createdPrinter.CreatedAt,
                    UpdatedAt = createdPrinter.UpdatedAt
                };
                
                return CreatedAtAction(nameof(GetById), new { id = createdPrinter.Id }, result);
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
                existingPrinter.IpAddress = printerDto.IpAddress;
                existingPrinter.Location = printerDto.Location;
                existingPrinter.Model = printerDto.Model;
                existingPrinter.Status = printerDto.Status;
                existingPrinter.StatusMessage = printerDto.StatusMessage ?? $"Actualizado el {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                existingPrinter.IsOnline = printerDto.IsOnline;
                existingPrinter.IsActive = printerDto.IsActive;
                existingPrinter.UpdatedAt = DateTime.UtcNow;

                var updated = await _printerService.UpdatePrinterAsync(existingPrinter);
                if (!updated)
                    return StatusCode(500, "Error al actualizar la impresora");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la impresora con ID: {PrinterId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _printerService.DeletePrinterAsync(id);
                if (!deleted)
                    return NotFound();
                
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
                var printer = await _printerService.GetPrinterByIdAsync(id);
                if (printer == null)
                    return NotFound();

                var status = new PrinterStatusDto
                {
                    PrinterId = printer.Id,
                    Status = printer.Status,
                    TonerLevel = "100%",
                    TonerLevelPercentage = 100,
                    LastUpdate = printer.LastStatusUpdate ?? DateTime.UtcNow,
                    IsOnline = printer.IsOnline,
                    Name = printer.Name,
                    IpAddress = printer.IpAddress,
                    StatusMessage = printer.StatusMessage ?? $"Estado actual: {printer.Status}",
                    Metrics = new Dictionary<string, string>
                    {
                        { "Modelo", printer.Model },
                        { "Ubicación", printer.Location ?? "No especificada" },
                        { "Estado", printer.Status.ToString() },
                        { "Última actualización", printer.LastStatusUpdate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Nunca" },
                        { "En línea", printer.IsOnline ? "Sí" : "No" }
                    }
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la impresora con ID {PrinterId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
