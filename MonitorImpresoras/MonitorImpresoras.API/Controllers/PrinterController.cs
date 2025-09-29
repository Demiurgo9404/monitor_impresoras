using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.DTOs.Printers;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]  // Requiere autenticación para todos los endpoints
    public class PrinterController : ControllerBase
    {
        private readonly IPrinterService _service;
        private readonly IMapper _mapper;

        public PrinterController(IPrinterService service, IMapper mapper)
        {
            _printerService = printerService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las impresoras
        /// </summary>
        /// <returns>Lista de impresoras</returns>
        [HttpGet]
        [Authorize(Policy = "CanReadPrinters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPrinters()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Obteniendo lista de impresoras para usuario: {UserId}", userId);

                var printers = await _printerService.GetAllPrintersAsync();
                var printerDtos = _mapper.Map<IEnumerable<PrinterDto>>(printers);

                _logger.LogInformation("Se obtuvieron {Count} impresoras", printerDtos.Count());
                return Ok(printerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de impresoras");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una impresora por ID
        /// </summary>
        /// <param name="id">ID de la impresora</param>
        /// <returns>Impresora encontrada</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanReadPrinters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPrinter(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Obteniendo impresora {PrinterId} para usuario: {UserId}", id, userId);

                var printer = await _printerService.GetPrinterByIdAsync(id);
                if (printer == null)
                {
                    return NotFound(new { errorCode = "PrinterNotFound", message = "Impresora no encontrada" });
                }

                var printerDto = _mapper.Map<PrinterDto>(printer);
                _logger.LogInformation("Impresora {PrinterId} obtenida exitosamente", id);
                return Ok(printerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener impresora {PrinterId}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva impresora
        /// </summary>
        /// <param name="createPrinterDto">Datos de la impresora</param>
        /// <returns>Impresora creada</returns>
        [HttpPost]
        [Authorize(Policy = "CanWritePrinters")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePrinter(CreatePrinterDto createPrinterDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Creando nueva impresora por usuario: {UserId}", userId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var printer = _mapper.Map<Printer>(createPrinterDto);
                var createdPrinter = await _printerService.CreatePrinterAsync(printer);

                var printerDto = _mapper.Map<PrinterDto>(createdPrinter);

                _logger.LogInformation("Impresora creada exitosamente: {PrinterId} por usuario: {UserId}", createdPrinter.Id, userId);
                return CreatedAtAction(nameof(GetPrinter), new { id = createdPrinter.Id }, printerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear impresora");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una impresora existente
        /// </summary>
        /// <param name="id">ID de la impresora</param>
        /// <param name="updatePrinterDto">Datos de actualización</param>
        /// <returns>Impresora actualizada</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanWritePrinters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePrinter(Guid id, UpdatePrinterDto updatePrinterDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Actualizando impresora {PrinterId} por usuario: {UserId}", id, userId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var printer = _mapper.Map<Printer>(updatePrinterDto);
                var updatedPrinter = await _printerService.UpdatePrinterAsync(id, printer);

                if (updatedPrinter == null)
                {
                    return NotFound(new { errorCode = "PrinterNotFound", message = "Impresora no encontrada" });
                }

                var printerDto = _mapper.Map<PrinterDto>(updatedPrinter);
                _logger.LogInformation("Impresora {PrinterId} actualizada exitosamente por usuario: {UserId}", id, userId);
                return Ok(printerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar impresora {PrinterId}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina una impresora
        /// </summary>
        /// <param name="id">ID de la impresora</param>
        /// <returns>Resultado de la eliminación</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanWritePrinters")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePrinter(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Eliminando impresora {PrinterId} por usuario: {UserId}", id, userId);

                var result = await _printerService.DeletePrinterAsync(id);
                if (!result)
                {
                    return NotFound(new { errorCode = "PrinterNotFound", message = "Impresora no encontrada" });
                }

                _logger.LogInformation("Impresora {PrinterId} eliminada exitosamente por usuario: {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar impresora {PrinterId}", id);
                throw;
            }
        }
    }
}
