using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using System.Security.Claims;

namespace QOPIQ.API.Controllers
{
    /// <summary>
    /// Controlador para gestión de reportes programados
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class ScheduledReportController : ControllerBase
    {
        private readonly IScheduledReportService _scheduledReportService;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<ScheduledReportController> _logger;

        public ScheduledReportController(
            IScheduledReportService scheduledReportService,
            ITenantAccessor tenantAccessor,
            ILogger<ScheduledReportController> logger)
        {
            _scheduledReportService = scheduledReportService;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los reportes programados del tenant actual
        /// </summary>
        /// <returns>Lista de reportes programados</returns>
        [HttpGet]
        public async Task<ActionResult<List<ScheduledReportDto>>> GetScheduledReports()
        {
            try
            {
                var scheduledReports = await _scheduledReportService.GetScheduledReportsAsync();
                
                _logger.LogInformation("Retrieved {Count} scheduled reports for tenant {TenantId}", 
                    scheduledReports.Count, _tenantAccessor.TenantId);
                
                return Ok(scheduledReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scheduled reports");
                return StatusCode(500, new { message = "Error retrieving scheduled reports" });
            }
        }

        /// <summary>
        /// Obtiene un reporte programado específico por ID
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <returns>Información del reporte programado</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduledReportDto>> GetScheduledReport(Guid id)
        {
            try
            {
                var scheduledReport = await _scheduledReportService.GetScheduledReportByIdAsync(id);
                if (scheduledReport == null)
                {
                    return NotFound(new { message = "Scheduled report not found" });
                }

                return Ok(scheduledReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scheduled report {ScheduledReportId}", id);
                return StatusCode(500, new { message = "Error retrieving scheduled report" });
            }
        }

        /// <summary>
        /// Crea un nuevo reporte programado (solo Admin y ProjectManager)
        /// </summary>
        /// <param name="createDto">Datos del nuevo reporte programado</param>
        /// <returns>Reporte programado creado</returns>
        [HttpPost]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult<ScheduledReportDto>> CreateScheduledReport([FromBody] CreateScheduledReportDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var scheduledReport = await _scheduledReportService.CreateScheduledReportAsync(createDto);
                
                _logger.LogInformation("Scheduled report created: {ScheduledReportId} by user {UserId}", 
                    scheduledReport.Id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return CreatedAtAction(nameof(GetScheduledReport), new { id = scheduledReport.Id }, scheduledReport);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized scheduled report creation: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid scheduled report creation data: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scheduled report");
                return StatusCode(500, new { message = "Error creating scheduled report" });
            }
        }

        /// <summary>
        /// Actualiza un reporte programado existente (solo Admin y ProjectManager)
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <param name="updateDto">Datos actualizados</param>
        /// <returns>Reporte programado actualizado</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult<ScheduledReportDto>> UpdateScheduledReport(Guid id, [FromBody] CreateScheduledReportDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var scheduledReport = await _scheduledReportService.UpdateScheduledReportAsync(id, updateDto);
                if (scheduledReport == null)
                {
                    return NotFound(new { message = "Scheduled report not found" });
                }

                _logger.LogInformation("Scheduled report updated: {ScheduledReportId} by user {UserId}", 
                    id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(scheduledReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scheduled report {ScheduledReportId}", id);
                return StatusCode(500, new { message = "Error updating scheduled report" });
            }
        }

        /// <summary>
        /// Elimina un reporte programado (solo SuperAdmin)
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = QopiqRoles.SuperAdmin)]
        public async Task<ActionResult> DeleteScheduledReport(Guid id)
        {
            try
            {
                var result = await _scheduledReportService.DeleteScheduledReportAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Scheduled report not found" });
                }

                _logger.LogInformation("Scheduled report deleted: {ScheduledReportId} by user {UserId}", 
                    id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { message = "Scheduled report deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting scheduled report {ScheduledReportId}", id);
                return StatusCode(500, new { message = "Error deleting scheduled report" });
            }
        }

        /// <summary>
        /// Activa o desactiva un reporte programado
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <param name="isActive">Estado activo/inactivo</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult> ToggleScheduledReport(Guid id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _scheduledReportService.ToggleScheduledReportAsync(id, isActive);
                if (!result)
                {
                    return NotFound(new { message = "Scheduled report not found" });
                }

                _logger.LogInformation("Scheduled report {Action}: {ScheduledReportId} by user {UserId}", 
                    isActive ? "activated" : "deactivated", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { 
                    message = $"Scheduled report {(isActive ? "activated" : "deactivated")} successfully",
                    isActive 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling scheduled report {ScheduledReportId}", id);
                return StatusCode(500, new { message = "Error toggling scheduled report" });
            }
        }

        /// <summary>
        /// Ejecuta manualmente un reporte programado
        /// </summary>
        /// <param name="id">ID del reporte programado</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/execute")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult> ExecuteScheduledReport(Guid id)
        {
            try
            {
                var result = await _scheduledReportService.ExecuteScheduledReportAsync(id);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to execute scheduled report" });
                }

                _logger.LogInformation("Scheduled report executed manually: {ScheduledReportId} by user {UserId}", 
                    id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { message = "Scheduled report executed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scheduled report {ScheduledReportId}", id);
                return StatusCode(500, new { message = "Error executing scheduled report" });
            }
        }

        /// <summary>
        /// Obtiene plantillas de horarios predefinidos
        /// </summary>
        /// <returns>Lista de plantillas de horarios</returns>
        [HttpGet("schedule-templates")]
        public ActionResult<object> GetScheduleTemplates()
        {
            var templates = new[]
            {
                new { name = "Diario", cron = "0 0 * * *", description = "Todos los días a medianoche" },
                new { name = "Semanal (Lunes)", cron = "0 0 * * 1", description = "Todos los lunes a medianoche" },
                new { name = "Mensual", cron = "0 0 1 * *", description = "El primer día de cada mes" },
                new { name = "Trimestral", cron = "0 0 1 1,4,7,10 *", description = "Cada trimestre" },
                new { name = "Anual", cron = "0 0 1 1 *", description = "El primer día del año" },
                new { name = "Cada hora", cron = "0 * * * *", description = "Cada hora en punto" },
                new { name = "Cada 6 horas", cron = "0 */6 * * *", description = "Cada 6 horas" },
                new { name = "Lunes a Viernes", cron = "0 0 * * 1-5", description = "Días laborables a medianoche" }
            };

            return Ok(new
            {
                scheduleTemplates = templates,
                cronHelp = new
                {
                    format = "minuto hora día mes día_semana",
                    examples = new
                    {
                        daily = "0 0 * * *",
                        weekly = "0 0 * * 1",
                        monthly = "0 0 1 * *"
                    }
                }
            });
        }

        /// <summary>
        /// Obtiene estadísticas de reportes programados
        /// </summary>
        /// <returns>Estadísticas de reportes programados</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetScheduledReportStats()
        {
            try
            {
                var scheduledReports = await _scheduledReportService.GetScheduledReportsAsync();
                
                var stats = new
                {
                    totalScheduledReports = scheduledReports.Count,
                    activeReports = scheduledReports.Count(sr => sr.IsActive),
                    inactiveReports = scheduledReports.Count(sr => !sr.IsActive),
                    reportsByType = scheduledReports
                        .GroupBy(sr => sr.ReportType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    reportsByFormat = scheduledReports
                        .GroupBy(sr => sr.Format)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    nextExecutions = scheduledReports
                        .Where(sr => sr.IsActive && sr.NextRun.HasValue)
                        .OrderBy(sr => sr.NextRun)
                        .Take(5)
                        .Select(sr => new { 
                            sr.Id, 
                            sr.Name, 
                            sr.NextRun,
                            sr.ReportType 
                        })
                        .ToList(),
                    recentExecutions = scheduledReports
                        .Where(sr => sr.LastRun.HasValue)
                        .OrderByDescending(sr => sr.LastRun)
                        .Take(5)
                        .Select(sr => new { 
                            sr.Id, 
                            sr.Name, 
                            sr.LastRun,
                            sr.ReportType 
                        })
                        .ToList()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scheduled report stats");
                return StatusCode(500, new { message = "Error retrieving scheduled report statistics" });
            }
        }

        /// <summary>
        /// Endpoint de prueba para validar autorización en reportes programados
        /// </summary>
        /// <returns>Información del contexto de autorización</returns>
        [HttpGet("test-auth")]
        public ActionResult GetAuthTest()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(QopiqClaims.Role)?.Value;
            var tenantId = _tenantAccessor.TenantId;

            return Ok(new
            {
                message = "Scheduled report controller authorization successful",
                userId,
                role,
                tenantId,
                canCreateScheduledReports = User.IsInRole(QopiqRoles.CompanyAdmin) || 
                                           User.IsInRole(QopiqRoles.SuperAdmin) || 
                                           User.IsInRole(QopiqRoles.ProjectManager),
                canDeleteScheduledReports = User.IsInRole(QopiqRoles.SuperAdmin),
                canExecuteScheduledReports = User.IsInRole(QopiqRoles.CompanyAdmin) || 
                                            User.IsInRole(QopiqRoles.SuperAdmin) || 
                                            User.IsInRole(QopiqRoles.ProjectManager),
                timestamp = DateTime.UtcNow
            });
        }
    }
}

