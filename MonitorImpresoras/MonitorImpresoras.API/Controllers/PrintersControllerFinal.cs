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

        /// <summary>
        /// Devuelve la lista de impresoras (baseline: datos dummy desde servicio)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrinterDto>>> GetAll(CancellationToken ct)
            => Ok(await _svc.GetAllAsync(ct));

        /// <summary>
        /// Devuelve una impresora por id (baseline dummy)
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PrinterDto>> GetById(int id, CancellationToken ct)
        {
            var item = await _svc.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }
    }
}
