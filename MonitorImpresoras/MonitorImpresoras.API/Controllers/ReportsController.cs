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
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los reportes disponibles para el usuario autenticado
        /// </summary>
        /// <returns>Lista de reportes disponibles</returns>
        [HttpGet("available")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableReports()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Obteniendo reportes disponibles para usuario: {UserId}", userId);

                var reports = await _reportService.GetAvailableReportsAsync(userId);

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes disponibles");
                throw;
            }
        }

        /// <summary>
        /// Genera un reporte de forma asíncrona
        /// </summary>
        /// <param name="request">Parámetros del reporte</param>
        /// <returns>Información de la ejecución del reporte</returns>
        [HttpPost("generate")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                _logger.LogInformation("Generando reporte para usuario: {UserId}, Template: {TemplateId}",
                    userId, request.ReportTemplateId);

                var result = await _reportService.GenerateReportAsync(request, userId, ipAddress);

                return Accepted(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso denegado para generar reporte");
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Parámetros inválidos para generar reporte");
                return BadRequest(new { errorCode = "InvalidParameters", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de reportes del usuario autenticado
        /// </summary>
        /// <param name="page">Número de página (por defecto 1)</param>
        /// <param name="pageSize">Tamaño de página (por defecto 20)</param>
        /// <returns>Historial de ejecuciones de reportes</returns>
        [HttpGet("history")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReportHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Obteniendo historial de reportes para usuario: {UserId}", userId);

                var history = await _reportService.GetReportHistoryAsync(userId, page, pageSize);

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de reportes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene detalles de una ejecución específica de reporte
        /// </summary>
        /// <param name="executionId">ID de la ejecución</param>
        /// <returns>Detalles de la ejecución</returns>
        [HttpGet("{executionId}")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReportExecution(int executionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Obteniendo detalles de ejecución {ExecutionId} para usuario {UserId}",
                    executionId, userId);

                var execution = await _reportService.GetReportExecutionAsync(executionId, userId);

                if (execution == null)
                {
                    return NotFound(new { errorCode = "ExecutionNotFound", message = "Ejecución de reporte no encontrada" });
                }

                return Ok(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de ejecución {ExecutionId}", executionId);
                throw;
            }
        }

        /// <summary>
        /// Descarga el contenido de un reporte generado
        /// </summary>
        /// <param name="executionId">ID de la ejecución</param>
        /// <returns>Archivo del reporte</returns>
        [HttpGet("{executionId}/download")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadReport(int executionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Descargando reporte {ExecutionId} para usuario {UserId}", executionId, userId);

                var (content, contentType, fileName) = await _reportService.DownloadReportAsync(executionId, userId);

                return File(content, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Reporte no encontrado para descarga: {ExecutionId}", executionId);
                return NotFound(new { errorCode = "ReportNotFound", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Reporte no disponible para descarga: {ExecutionId}", executionId);
                return BadRequest(new { errorCode = "ReportNotReady", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar reporte {ExecutionId}", executionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de reportes del usuario autenticado
        /// </summary>
        /// <returns>Estadísticas de reportes</returns>
        [HttpGet("statistics")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReportStatistics()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Obteniendo estadísticas de reportes para usuario: {UserId}", userId);

                var statistics = await _reportService.GetReportStatisticsAsync(userId);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de reportes");
                throw;
            }
        }

        /// <summary>
        /// Cancela una ejecución de reporte en progreso
        /// </summary>
        /// <param name="executionId">ID de la ejecución</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{executionId}/cancel")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelReportExecution(int executionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Cancelando ejecución {ExecutionId} para usuario {UserId}", executionId, userId);

                var result = await _reportService.CancelReportExecutionAsync(executionId, userId);

                if (!result)
                {
                    return BadRequest(new { errorCode = "CannotCancel", message = "No se puede cancelar la ejecución" });
                }

                return Ok(new { success = true, message = "Ejecución cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar ejecución {ExecutionId}", executionId);
                throw;
            }
        }
    }
}
