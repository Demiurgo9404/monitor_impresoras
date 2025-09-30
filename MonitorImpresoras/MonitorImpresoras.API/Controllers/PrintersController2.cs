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
    public sealed class PrintersController : ControllerBase
    {
        private readonly IPrinterQueryService _svc;
        public PrintersController(IPrinterQueryService svc) => _svc = svc;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrinterDto>>> GetAll(CancellationToken ct)
        {
            var printers = await _svc.GetAllAsync(ct);
            return Ok(printers);
        }
    }
}
