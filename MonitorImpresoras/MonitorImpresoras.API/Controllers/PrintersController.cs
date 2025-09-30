using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PrintersController : ControllerBase
    {
        private readonly IPrinterQueryService _svc;

        public PrintersController(IPrinterQueryService svc)
        {
            _svc = svc;
        }

        /// <summary>
        /// Devuelve la lista de impresoras (baseline: datos dummy desde servicio)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrinterDto>>> GetAll(CancellationToken ct)
        {
            var printers = await _svc.GetAllAsync(ct);
            return Ok(printers);
                var printer = _mapper.Map<Printer>(createPrinterDto);
                var addedPrinter = await _printerService.AddPrinterAsync(printer);
                var printerDto = _mapper.Map<PrinterDto>(addedPrinter);
                
                return CreatedAtAction(
                    nameof(GetPrinterById), 
                    new { id = printerDto.Id }, 
                    printerDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al agregar impresora: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar una nueva impresora");
                return StatusCode(500, new { Message = "Error interno del servidor al agregar la impresora", Error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una impresora existente
        /// </summary>
        /// <param name="id">ID de la impresora a actualizar</param>
        /// <param name="updatePrinterDto">Datos actualizados de la impresora</param>
        /// <returns>Sin contenido</returns>
        /// <response code="204">La impresora se actualizó correctamente</response>
        /// <response code="400">Los datos de la impresora no son válidos</response>
        /// <response code="404">No se encontró la impresora con el ID especificado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePrinter(int id, UpdatePrinterDto updatePrinterDto)
        {
            try
            {
                if (id != updatePrinterDto.Id)
                {
                    return BadRequest(new { Message = "El ID de la URL no coincide con el ID de la impresora" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Datos de la impresora no válidos", Errors = ModelState.Values.SelectMany(v => v.Errors) });
                }

                var printer = await _printerService.GetPrinterByIdAsync(id);
                if (printer == null)
                {
                    return NotFound(new { Message = $"No se encontró la impresora con ID {id}" });
                }

                _mapper.Map(updatePrinterDto, printer);
                printer.UpdatedAt = DateTime.UtcNow;
                
                await _printerService.UpdatePrinterAsync(printer);
                
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"No se encontró la impresora con ID {id} para actualizar");
                return NotFound(new { Message = $"No se encontró la impresora con ID {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar la impresora con ID {id}");
                return StatusCode(500, new { Message = "Error interno del servidor al actualizar la impresora", Error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una impresora
        /// </summary>
        /// <param name="id">ID de la impresora a eliminar</param>
        /// <returns>Sin contenido</returns>
        /// <response code="204">La impresora se eliminó correctamente</response>
        /// <response code="404">No se encontró la impresora con el ID especificado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePrinter(int id)
        {
            try
            {
                await _printerService.DeletePrinterAsync(id);
                _logger.LogInformation($"Impresora con ID {id} eliminada correctamente");
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"No se encontró la impresora con ID {id} para eliminar");
                return NotFound(new { Message = $"No se encontró la impresora con ID {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar la impresora con ID {id}");
                return StatusCode(500, new { Message = "Error interno del servidor al eliminar la impresora", Error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica el estado de una impresora
        /// </summary>
        /// <param name="id">ID de la impresora a verificar</param>
        /// <returns>Estado actualizado de la impresora</returns>
        /// <response code="200">Estado de la impresora verificado correctamente</response>
        /// <response code="404">No se encontró la impresora con el ID especificado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PrinterStatusDto>> CheckPrinterStatus(int id)
        {
            try
            {
                // Primero verificamos que la impresora exista
                var printer = await _printerService.GetPrinterByIdAsync(id);
                if (printer == null)
                {
                    return NotFound(new { Message = $"No se encontró la impresora con ID {id}" });
                }
                
                // Verificamos el estado de la impresora
                await _printerService.CheckPrinterStatusAsync(id);
                
                // Obtenemos la impresora actualizada
                var updatedPrinter = await _printerService.GetPrinterByIdAsync(id);
                var statusDto = _mapper.Map<PrinterStatusDto>(updatedPrinter);
                
                return Ok(new 
                { 
                    Message = "Estado de la impresora verificado correctamente",
                    Data = statusDto
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"No se encontró la impresora con ID {id} para verificar su estado");
                return NotFound(new { Message = $"No se encontró la impresora con ID {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar el estado de la impresora con ID {id}");
                return StatusCode(500, new { Message = "Error interno del servidor al verificar el estado de la impresora", Error = ex.Message });
            }
        }
    }
}
