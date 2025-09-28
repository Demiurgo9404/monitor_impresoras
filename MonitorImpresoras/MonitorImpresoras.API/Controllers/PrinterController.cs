using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.DTOs.Printers;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// Obtiene todas las impresoras
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var printers = await _service.GetAllPrintersAsync();
            var printerDtos = _mapper.Map<IEnumerable<PrinterDto>>(printers);
            return Ok(printerDtos);
        }

        /// <summary>
        /// Obtiene una impresora por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var printer = await _service.GetPrinterByIdAsync(id);
            if (printer == null)
                return NotFound($"Impresora con ID {id} no encontrada");

            var printerDto = _mapper.Map<PrinterDto>(printer);
            return Ok(printerDto);
        }

        /// <summary>
        /// Crea una nueva impresora
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(CreatePrinterDto createDto)
        {
            var printer = _mapper.Map<Printer>(createDto);
            var createdPrinter = await _service.CreatePrinterAsync(printer);
            var resultDto = _mapper.Map<PrinterDto>(createdPrinter);

            return CreatedAtAction(nameof(GetById), new { id = createdPrinter.Id }, resultDto);
        }

        /// <summary>
        /// Actualiza una impresora existente
        /// </summary>
        [HttpPut("{id}")]
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
        /// Elimina una impresora
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeletePrinterAsync(id);
            if (!result)
                return NotFound($"Impresora con ID {id} no encontrada");

            return NoContent();
        }
    }
}
