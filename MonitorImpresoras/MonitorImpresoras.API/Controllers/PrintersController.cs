using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using System;
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
        /// Devuelve la lista de Impresoras (EF Core)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrinterQueryDto>>> GetAll(CancellationToken ct)
            => Ok(await _svc.GetAllAsync(ct));

        /// <summary>
        /// Devuelve una Impresora por id (Guid)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PrinterQueryDto>> GetById(Guid id, CancellationToken ct)
        {
            var item = await _svc.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>
        /// Búsqueda paginada de impresoras
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            if (page < 1 || pageSize < 1 || pageSize > 200) return BadRequest("Invalid paging.");
            var (items, total) = await _svc.SearchAsync(q, page, pageSize, ct);
            return Ok(new { total, page, pageSize, items });
        }
    }
}
