using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrinterController : ControllerBase
    {
        private readonly IPrinterService _service;

        public PrinterController(IPrinterService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllPrintersAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var printer = await _service.GetPrinterByIdAsync(id);
            return printer == null ? NotFound() : Ok(printer);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Printer printer)
        {
            var created = await _service.CreatePrinterAsync(printer);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Printer updatedPrinter)
        {
            var printer = await _service.UpdatePrinterAsync(id, updatedPrinter);
            return printer == null ? NotFound() : Ok(printer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeletePrinterAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
