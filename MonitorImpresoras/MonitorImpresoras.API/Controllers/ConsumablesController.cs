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
    public class ConsumablesController : BaseApiController
    {
        private readonly IConsumableService _consumableService;
        private readonly ILogger<ConsumablesController> _logger;

        public ConsumablesController(
            IConsumableService consumableService,
            ILogger<ConsumablesController> logger)
        {
            _consumableService = consumableService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<ConsumableDTO>>> GetConsumables(
            [FromQuery] int? printerId = null,
            [FromQuery] string type = null,
            [FromQuery] string status = null,
            [FromQuery] bool? needsReplacement = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new ConsumableFilterDTO
            {
                PrinterId = printerId,
                Type = type,
                Status = status,
                NeedsReplacement = needsReplacement,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var consumables = await _consumableService.GetConsumablesByFilterAsync(filter);
            // Note: For pagination, you might want to update this to use HandlePagedResult
            return HandleResult(consumables);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<ConsumableDTO>> GetConsumable(int id)
        {
            var consumable = await _consumableService.GetConsumableByIdAsync(id);
            return HandleResult(consumable);
        }

        [HttpGet("printer/{printerId}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<ConsumableDTO>>> GetConsumablesByPrinter(int printerId)
        {
            var consumables = await _consumableService.GetConsumablesByPrinterIdAsync(printerId);
            return HandleResult(consumables);
        }

        [HttpPut("{id}/level")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ConsumableDTO>> UpdateConsumableLevel(int id, [FromBody] UpdateConsumableLevelDTO updateDto)
        {
            if (id != updateDto.ConsumableId)
                return BadRequest("ID de la ruta no coincide con el ID del consumible");

            var result = await _consumableService.UpdateConsumableLevelAsync(updateDto);
            return HandleResult(result);
        }

        [HttpPut("{id}/reset")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ConsumableDTO>> ResetConsumable(int id)
        {
            var updateDto = new UpdateConsumableLevelDTO
            {
                ConsumableId = id,
                ResetCounter = true
            };

            var result = await _consumableService.UpdateConsumableLevelAsync(updateDto);
            return HandleResult(result);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<ConsumableStatsDTO>> GetConsumableStats()
        {
            var stats = await _consumableService.GetConsumableStatsAsync();
            return HandleResult(stats);
        }

        [HttpPost("check-low-levels")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> CheckLowConsumables()
        {
            await _consumableService.CheckAndCreateLowConsumableAlertsAsync();
            return Ok("Verificación de consumibles bajos completada");
        }
    }
}
