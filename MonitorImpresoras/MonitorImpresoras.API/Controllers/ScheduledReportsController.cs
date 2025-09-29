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
    public class ScheduledReportsController : ControllerBase
    {
        private readonly IScheduledReportService _scheduledReportService;
        private readonly ILogger<ScheduledReportsController> _logger;

        public ScheduledReportsController(
            IScheduledReportService scheduledReportService,
            ILogger<ScheduledReportsController> logger)
        {
            _scheduledReportService = scheduledReportService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los reportes programados del usuario autenticado
        /// </summary>
        /// <returns>Lista de reportes programados</returns>
        [HttpGet]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetScheduledReports()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Obteniendo reportes programados para usuario: {UserId}", userId);

                var reports = await _scheduledReportService.GetUserScheduledReportsAsync(userId);

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes programados");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un reporte programado específico
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <returns>Detalles del reporte programado</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetScheduledReport(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Obteniendo reporte programado {ScheduledReportId} para usuario {UserId}", id, userId);

                var report = await _scheduledReportService.GetScheduledReportByIdAsync(id, userId);

                if (report == null)
                {
                    return NotFound(new { errorCode = "ScheduledReportNotFound", message = "Reporte programado no encontrado" });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte programado {ScheduledReportId}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo reporte programado
        /// </summary>
        /// <param name="request">Datos del reporte programado</param>
        /// <returns>Reporte programado creado</returns>
        [HttpPost]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateScheduledReport([FromBody] CreateScheduledReportDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Creando reporte programado para usuario: {UserId}", userId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var report = await _scheduledReportService.CreateScheduledReportAsync(request, userId);

                _logger.LogInformation("Reporte programado {ScheduledReportId} creado exitosamente", report.Id);
                return CreatedAtAction(nameof(GetScheduledReport), new { id = report.Id }, report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Parámetros inválidos para crear reporte programado");
                return BadRequest(new { errorCode = "InvalidParameters", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reporte programado");
                throw;
            }
        }

        /// <summary>
        /// Actualiza un reporte programado existente
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <param name="request">Datos de actualización</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateScheduledReport(int id, [FromBody] UpdateScheduledReportDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Actualizando reporte programado {ScheduledReportId} para usuario {UserId}", id, userId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _scheduledReportService.UpdateScheduledReportAsync(id, request, userId);

                if (!result)
                {
                    return NotFound(new { errorCode = "ScheduledReportNotFound", message = "Reporte programado no encontrado" });
                }

                return Ok(new { success = true, message = "Reporte programado actualizado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Parámetros inválidos para actualizar reporte programado");
                return BadRequest(new { errorCode = "InvalidParameters", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reporte programado {ScheduledReportId}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina un reporte programado
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteScheduledReport(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Eliminando reporte programado {ScheduledReportId} para usuario {UserId}", id, userId);

                var result = await _scheduledReportService.DeleteScheduledReportAsync(id, userId);

                if (!result)
                {
                    return NotFound(new { errorCode = "ScheduledReportNotFound", message = "Reporte programado no encontrado" });
                }

                return Ok(new { success = true, message = "Reporte programado eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reporte programado {ScheduledReportId}", id);
                throw;
            }
        }

        /// <summary>
        /// Ejecuta manualmente un reporte programado
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <returns>Resultado de la ejecución</returns>
        [HttpPost("{id}/execute")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExecuteScheduledReport(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Ejecutando manualmente reporte programado {ScheduledReportId} para usuario {UserId}", id, userId);

                var report = await _scheduledReportService.GetScheduledReportByIdAsync(id, userId);

                if (report == null)
                {
                    return NotFound(new { errorCode = "ScheduledReportNotFound", message = "Reporte programado no encontrado" });
                }

                // Crear reporte manualmente
                var reportRequest = new ReportRequestDto
                {
                    ReportTemplateId = report.ReportTemplateId,
                    Format = report.Format,
                    Parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(report.FixedParameters ?? "{}")
                };

                // Usar el servicio de reportes para generar
                using var scope = HttpContext.RequestServices.CreateScope();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var result = await reportService.GenerateReportAsync(reportRequest, userId, ipAddress);

                return Accepted(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar manualmente reporte programado {ScheduledReportId}", id);
                throw;
            }
        }
    }
}
