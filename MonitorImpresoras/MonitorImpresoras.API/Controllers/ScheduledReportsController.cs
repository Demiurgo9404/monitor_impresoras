using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduledReportsController : ControllerBase
    {
        private readonly IScheduledReportService _scheduledReportService;
        private readonly IReportTemplateService _templateService;
        private readonly IReportSchedulerService _schedulerService;

        public ScheduledReportsController(
            IScheduledReportService scheduledReportService,
            IReportTemplateService templateService,
            IReportSchedulerService schedulerService)
        {
            _scheduledReportService = scheduledReportService;
            _templateService = templateService;
            _schedulerService = schedulerService;
        }

        /// <summary>
        /// Obtiene todos los reportes programados del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="includeInactive">Incluir reportes inactivos</param>
        /// <returns>Lista de reportes programados</returns>
        [HttpGet]
        public async Task<IActionResult> GetScheduledReports([FromQuery] int tenantId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var reports = await _scheduledReportService.GetScheduledReportsAsync(tenantId, includeInactive);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un reporte programado por ID
        /// </summary>
        /// <param name="reportId">ID del reporte</param>
        /// <returns>Reporte programado</returns>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetScheduledReport(int reportId)
        {
            try
            {
                var report = await _scheduledReportService.GetScheduledReportAsync(reportId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo reporte programado
        /// </summary>
        /// <param name="request">Datos del reporte</param>
        /// <returns>Reporte creado</returns>
        [HttpPost]
        public async Task<IActionResult> CreateScheduledReport([FromBody] CreateScheduledReportRequestDTO request)
        {
            try
            {
                // Asignar tenant ID del usuario autenticado (por simplicidad, usar 1)
                request.TenantId = 1;

                var report = await _scheduledReportService.CreateScheduledReportAsync(request);
                return CreatedAtAction(nameof(GetScheduledReport), new { id = report.Id }, report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un reporte programado
        /// </summary>
        /// <param name="reportId">ID del reporte</param>
        /// <param name="request">Datos actualizados</param>
        /// <returns>Reporte actualizado</returns>
        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateScheduledReport(int reportId, [FromBody] UpdateScheduledReportRequestDTO request)
        {
            try
            {
                var report = await _scheduledReportService.UpdateScheduledReportAsync(reportId, request);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un reporte programado
        /// </summary>
        /// <param name="reportId">ID del reporte</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteScheduledReport(int reportId)
        {
            try
            {
                await _scheduledReportService.DeleteScheduledReportAsync(reportId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Activa o desactiva un reporte programado
        /// </summary>
        /// <param name="reportId">ID del reporte</param>
        /// <param name="isActive">Estado deseado</param>
        /// <returns>Reporte actualizado</returns>
        [HttpPatch("{reportId}/status")]
        public async Task<IActionResult> ToggleScheduledReportStatus(int reportId, [FromQuery] bool isActive)
        {
            try
            {
                var report = await _scheduledReportService.ToggleScheduledReportStatusAsync(reportId, isActive);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Ejecuta un reporte programado inmediatamente
        /// </summary>
        /// <param name="request">Datos de ejecución manual</param>
        /// <returns>Resultado de la ejecución</returns>
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteReport([FromBody] ManualReportRequestDTO request)
        {
            try
            {
                var result = await _scheduledReportService.ExecuteReportAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el estado de una ejecución de reporte
        /// </summary>
        /// <param name="executionId">ID de la ejecución</param>
        /// <returns>Estado de la ejecución</returns>
        [HttpGet("executions/{executionId}/status")]
        public async Task<IActionResult> GetExecutionStatus(int executionId)
        {
            try
            {
                var status = await _scheduledReportService.GetExecutionStatusAsync(executionId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de reportes del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Estadísticas de reportes</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetReportStatistics([FromQuery] int tenantId)
        {
            try
            {
                var statistics = await _scheduledReportService.GetReportStatisticsAsync(tenantId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene ejecuciones recientes de reportes
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="limit">Número máximo de ejecuciones</param>
        /// <returns>Ejecuciones recientes</returns>
        [HttpGet("executions/recent")]
        public async Task<IActionResult> GetRecentExecutions([FromQuery] int tenantId, [FromQuery] int limit = 10)
        {
            try
            {
                var executions = await _scheduledReportService.GetRecentExecutionsAsync(tenantId, limit);
                return Ok(executions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Busca reportes programados por criterios
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <param name="reportType">Tipo de reporte</param>
        /// <param name="status">Estado del reporte</param>
        /// <returns>Reportes encontrados</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchScheduledReports(
            [FromQuery] int tenantId,
            [FromQuery] string searchTerm,
            [FromQuery] string reportType,
            [FromQuery] string status)
        {
            try
            {
                var reports = await _scheduledReportService.SearchScheduledReportsAsync(tenantId, searchTerm, reportType, status);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene próximos reportes a ejecutar
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="limit">Número máximo de reportes</param>
        /// <returns>Próximos reportes</returns>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingReports([FromQuery] int tenantId, [FromQuery] int limit = 10)
        {
            try
            {
                var reports = await _scheduledReportService.GetUpcomingReportsAsync(tenantId, limit);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Duplica un reporte programado
        /// </summary>
        /// <param name="reportId">ID del reporte a duplicar</param>
        /// <param name="newName">Nuevo nombre para el reporte duplicado</param>
        /// <returns>Reporte duplicado</returns>
        [HttpPost("{reportId}/duplicate")]
        public async Task<IActionResult> DuplicateScheduledReport(int reportId, [FromQuery] string newName)
        {
            try
            {
                var report = await _scheduledReportService.DuplicateScheduledReportAsync(reportId, newName);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene configuraciones de reporte disponibles
        /// </summary>
        /// <param name="reportType">Tipo de reporte</param>
        /// <returns>Configuraciones disponibles</returns>
        [HttpGet("configurations")]
        public async Task<IActionResult> GetAvailableReportConfigurations([FromQuery] string reportType)
        {
            try
            {
                if (!Enum.TryParse<ReportType>(reportType, true, out var type))
                {
                    return BadRequest(new { error = "Tipo de reporte inválido" });
                }

                var configurations = await _scheduledReportService.GetAvailableReportConfigurationsAsync(type);
                return Ok(configurations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Valida la configuración de un reporte programado
        /// </summary>
        /// <param name="report">Reporte a validar</param>
        /// <returns>Lista de errores de validación</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateScheduledReport([FromBody] ScheduledReportDTO report)
        {
            try
            {
                var errors = await _scheduledReportService.ValidateScheduledReportAsync(report);
                return Ok(new { errors });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Limpia reportes antiguos según configuración de retención
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Número de reportes limpiados</returns>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupOldReports([FromQuery] int tenantId)
        {
            try
            {
                var cleanedCount = await _scheduledReportService.CleanupOldReportsAsync(tenantId);
                return Ok(new { cleanedCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Procesa todos los reportes programados pendientes
        /// </summary>
        /// <returns>Número de reportes procesados</returns>
        [HttpPost("process-pending")]
        public async Task<IActionResult> ProcessScheduledReports()
        {
            try
            {
                var processedCount = await _scheduledReportService.ProcessScheduledReportsAsync();
                return Ok(new { processedCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reintenta una ejecución fallida
        /// </summary>
        /// <param name="executionId">ID de la ejecución</param>
        /// <returns>Resultado del reintento</returns>
        [HttpPost("executions/{executionId}/retry")]
        public async Task<IActionResult> RetryFailedExecution(int executionId)
        {
            try
            {
                var result = await _scheduledReportService.RetryFailedExecutionAsync(executionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ========== TEMPLATES ==========

        /// <summary>
        /// Obtiene todas las plantillas disponibles
        /// </summary>
        /// <param name="includeInactive">Incluir plantillas inactivas</param>
        /// <returns>Lista de plantillas</returns>
        [HttpGet("templates")]
        public async Task<IActionResult> GetAllTemplates([FromQuery] bool includeInactive = false)
        {
            try
            {
                var templates = await _templateService.GetAllTemplatesAsync(includeInactive);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene plantillas por tipo de reporte
        /// </summary>
        /// <param name="reportType">Tipo de reporte</param>
        /// <returns>Plantillas del tipo especificado</returns>
        [HttpGet("templates/by-type/{reportType}")]
        public async Task<IActionResult> GetTemplatesByType(string reportType)
        {
            try
            {
                if (!Enum.TryParse<ReportType>(reportType, true, out var type))
                {
                    return BadRequest(new { error = "Tipo de reporte inválido" });
                }

                var templates = await _templateService.GetTemplatesByTypeAsync(type);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ========== SCHEDULER ==========

        /// <summary>
        /// Obtiene el estado del scheduler
        /// </summary>
        /// <returns>Estado del scheduler</returns>
        [HttpGet("scheduler/status")]
        public async Task<IActionResult> GetSchedulerStatus()
        {
            try
            {
                var status = await _schedulerService.GetStatusAsync();
                return Ok(new { status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Inicia el scheduler de reportes
        /// </summary>
        /// <returns>Confirmación</returns>
        [HttpPost("scheduler/start")]
        public async Task<IActionResult> StartScheduler()
        {
            try
            {
                await _schedulerService.StartAsync();
                return Ok(new { message = "Scheduler iniciado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Detiene el scheduler de reportes
        /// </summary>
        /// <returns>Confirmación</returns>
        [HttpPost("scheduler/stop")]
        public async Task<IActionResult> StopScheduler()
        {
            try
            {
                await _schedulerService.StopAsync();
                return Ok(new { message = "Scheduler detenido" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
