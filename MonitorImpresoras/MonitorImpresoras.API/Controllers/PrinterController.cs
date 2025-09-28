using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.DTOs.Printers;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Requiere autenticación para todos los endpoints
    public class PrinterController : ControllerBase
    {
        private readonly IPrinterService _service;
        private readonly IMapper _mapper;

        public PrinterController(IPrinterService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene todas las impresoras (requiere CanReadPrinters policy)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "CanReadPrinters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var printers = await _service.GetAllPrintersAsync();
            var printerDtos = _mapper.Map<IEnumerable<PrinterDto>>(printers);
            return Ok(printerDtos);
        }

        /// <summary>
        /// Obtiene una impresora por ID (requiere CanReadPrinters policy)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanReadPrinters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var printer = await _service.GetPrinterByIdAsync(id);
            if (printer == null)
                return NotFound($"Impresora con ID {id} no encontrada");

            var printerDto = _mapper.Map<PrinterDto>(printer);
            return Ok(printerDto);
        }

        /// <summary>
        /// Crea una nueva impresora (requiere CanWritePrinters policy)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CanWritePrinters")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(CreatePrinterDto createDto)
        {
            var printer = _mapper.Map<Printer>(createDto);
            var createdPrinter = await _service.CreatePrinterAsync(printer);
            var resultDto = _mapper.Map<PrinterDto>(createdPrinter);

            return CreatedAtAction(nameof(GetById), new { id = createdPrinter.Id }, resultDto);
        }

        /// <summary>
        /// Actualiza una impresora existente (requiere CanWritePrinters policy)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanWritePrinters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, UpdatePrinterDto updateDto)
        {
            var existingPrinter = await _service.GetPrinterByIdAsync(id);
            if (existingPrinter == null)
                return NotFound($"Impresora con ID {id} no encontrada");

            // Mapear solo los campos que no son null
            var updatedPrinter = _mapper.Map(updateDto, existingPrinter);

            var result = await _service.UpdatePrinterAsync(id, updatedPrinter);
            if (result == null)
                return NotFound($"Error al actualizar impresora con ID {id}");

            var resultDto = _mapper.Map<PrinterDto>(result);
            return Ok(resultDto);
        }

        /// <summary>
        /// Elimina una impresora (requiere Admin role)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeletePrinterAsync(id);
            if (!result)
                return NotFound($"Impresora con ID {id} no encontrada");

            return NoContent();
        }
    }
}
