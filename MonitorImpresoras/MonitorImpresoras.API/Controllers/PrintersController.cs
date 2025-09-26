using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    public class PrintersController : BaseApiController
    {
        private readonly IPrinterService _printerService;
        private readonly ILogger<PrintersController> _logger;
        private readonly IMapper _mapper;

        public PrintersController(
            IPrinterService printerService,
            ILogger<PrintersController> logger,
            IMapper mapper)
        {
            _printerService = printerService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<PrinterListDTO>>> GetPrinters()
        {
            var printers = await _printerService.GetAllPrintersAsync();
            return HandleResult(printers);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<PrinterDTO>> GetPrinter(int id)
        {
            var printer = await _printerService.GetPrinterByIdAsync(id);
            return HandleResult(printer);
        }

        [HttpGet("{id}/consumables")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<PrinterDTO>> GetPrinterWithConsumables(int id)
        {
            var printer = await _printerService.GetPrinterWithConsumablesAsync(id);
            return HandleResult(printer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<PrinterDTO>> CreatePrinter(CreatePrinterDTO printerDto)
        {
            var result = await _printerService.AddPrinterAsync(printerDto);
            return CreatedAtAction(nameof(GetPrinter), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> UpdatePrinter(int id, UpdatePrinterDTO printerDto)
        {
            if (id != printerDto.Id)
                return BadRequest("ID de la ruta no coincide con el ID del objeto");

            await _printerService.UpdatePrinterAsync(id, printerDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePrinter(int id)
        {
            await _printerService.DeletePrinterAsync(id);
            return NoContent();
        }

        [HttpGet("location/{location}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<PrinterListDTO>>> GetPrintersByLocation(string location)
        {
            var printers = await _printerService.GetPrintersByLocationAsync(location);
            return HandleResult(printers);
        }
    }
}
