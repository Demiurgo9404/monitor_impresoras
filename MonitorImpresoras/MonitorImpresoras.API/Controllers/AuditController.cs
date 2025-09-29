using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Security.Claims;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IExtendedAuditService _extendedAuditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(
            IExtendedAuditService extendedAuditService,
            ILogger<AuditController> logger)
        {
            _extendedAuditService = extendedAuditService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene eventos del sistema con filtros avanzados
        /// </summary>
        /// <param name="filter">Filtros para la consulta</param>
        /// <returns>Lista de eventos del sistema</returns>
        [HttpGet("events")]
        [Authorize(Policy = "RequireAdmin")] // Solo administradores pueden ver auditoría
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSystemEvents([FromQuery] SystemEventFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Obteniendo eventos del sistema con filtros");

                var events = await _extendedAuditService.GetSystemEventsAsync(
                    filter.EventType,
                    filter.Category,
                    filter.Severity,
                    filter.UserId,
                    filter.DateFrom,
                    filter.DateTo,
                    filter.Page,
                    filter.PageSize);

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener eventos del sistema");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de eventos del sistema
        /// </summary>
        /// <param name="dateFrom">Fecha de inicio del período</param>
        /// <param name="dateTo">Fecha de fin del período</param>
        /// <returns>Estadísticas de eventos del sistema</returns>
        [HttpGet("statistics")]
        [Authorize(Policy = "RequireAdmin")] // Solo administradores pueden ver estadísticas
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSystemEventStatistics([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de eventos del sistema");

                var statistics = await _extendedAuditService.GetSystemEventStatisticsAsync(dateFrom, dateTo);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de eventos del sistema");
                throw;
            }
        }

        /// <summary>
        /// Limpia eventos antiguos del sistema
        /// </summary>
        /// <param name="retentionDays">Días de retención (por defecto 90)</param>
        /// <returns>Número de eventos eliminados</returns>
        [HttpDelete("cleanup")]
        [Authorize(Policy = "RequireAdmin")] // Solo administradores pueden limpiar
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CleanupOldEvents([FromQuery] int retentionDays = 90)
        {
            try
            {
                _logger.LogInformation("Limpiando eventos antiguos del sistema");

                var deletedCount = await _extendedAuditService.CleanupOldEventsAsync(retentionDays);

                return Ok(new { deletedCount, message = $"{deletedCount} eventos antiguos eliminados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar eventos antiguos del sistema");
                throw;
            }
        }
    }
}
