using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using System.Security.Claims;

namespace QOPIQ.API.Controllers
{
    /// <summary>
    /// Controlador para gestión de reportes multi-tenant
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IReportService reportService,
            ITenantAccessor tenantAccessor,
            ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Genera un nuevo reporte
        /// </summary>
        /// <param name="generateDto">Datos para generar el reporte</param>
        /// <returns>Información del reporte generado</returns>
        [HttpPost("generate")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult<ReportDto>> GenerateReport([FromBody] GenerateReportDto generateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                _logger.LogInformation("Generating report for project {ProjectId} by user {UserId}", 
                    generateDto.ProjectId, userId);

                var report = await _reportService.GenerateReportAsync(generateDto);
                
                _logger.LogInformation("Report generated successfully: {ReportId}", report.Id);

                return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized report generation: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid report generation data: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return StatusCode(500, new { message = "Error generating report" });
            }
        }

        /// <summary>
        /// Obtiene lista de reportes con filtros y paginación
        /// </summary>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="projectId">Filtrar por proyecto</param>
        /// <param name="reportType">Filtrar por tipo de reporte</param>
        /// <param name="status">Filtrar por estado</param>
        /// <param name="format">Filtrar por formato</param>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Lista paginada de reportes</returns>
        [HttpGet]
        public async Task<ActionResult<ReportListDto>> GetReports(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? projectId = null,
            [FromQuery] string? reportType = null,
            [FromQuery] string? status = null,
            [FromQuery] string? format = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var filters = new ReportFiltersDto
                {
                    ProjectId = projectId,
                    ReportType = reportType,
                    Status = status,
                    Format = format,
                    SearchTerm = searchTerm
                };

                var result = await _reportService.GetReportsAsync(pageNumber, pageSize, filters);
                
                _logger.LogInformation("Retrieved {Count} reports for tenant {TenantId}", 
                    result.Reports.Count, _tenantAccessor.TenantId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports");
                return StatusCode(500, new { message = "Error retrieving reports" });
            }
        }

        /// <summary>
        /// Obtiene un reporte específico por ID
        /// </summary>
        /// <param name="id">ID del reporte</param>
        /// <returns>Información del reporte</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDto>> GetReport(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso al reporte
                var hasAccess = await _reportService.HasAccessToReportAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this report");
                }

                var report = await _reportService.GetReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound(new { message = "Report not found" });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {ReportId}", id);
                return StatusCode(500, new { message = "Error retrieving report" });
            }
        }

        /// <summary>
        /// Descarga el archivo de un reporte
        /// </summary>
        /// <param name="id">ID del reporte</param>
        /// <returns>Archivo del reporte</returns>
        [HttpGet("{id}/download")]
        public async Task<ActionResult> DownloadReport(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso al reporte
                var hasAccess = await _reportService.HasAccessToReportAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this report");
                }

                var (fileData, fileName, contentType) = await _reportService.DownloadReportAsync(id);
                
                _logger.LogInformation("Report downloaded: {ReportId} by user {UserId}", id, userId);

                return File(fileData, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Report download failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning("Report file not found: {Message}", ex.Message);
                return NotFound(new { message = "Report file not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading report {ReportId}", id);
                return StatusCode(500, new { message = "Error downloading report" });
            }
        }

        /// <summary>
        /// Elimina un reporte (solo SuperAdmin)
        /// </summary>
        /// <param name="id">ID del reporte</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = QopiqRoles.SuperAdmin)]
        public async Task<ActionResult> DeleteReport(Guid id)
        {
            try
            {
                var result = await _reportService.DeleteReportAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Report not found" });
                }

                _logger.LogInformation("Report deleted: {ReportId} by user {UserId}", 
                    id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { message = "Report deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
                return StatusCode(500, new { message = "Error deleting report" });
            }
        }

        /// <summary>
        /// Reenvía un reporte por email
        /// </summary>
        /// <param name="id">ID del reporte</param>
        /// <param name="emailRecipients">Lista de destinatarios</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/resend")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult> ResendReport(Guid id, [FromBody] string[] emailRecipients)
        {
            try
            {
                if (emailRecipients == null || !emailRecipients.Any())
                {
                    return BadRequest(new { message = "Email recipients are required" });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso al reporte
                var hasAccess = await _reportService.HasAccessToReportAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this report");
                }

                var result = await _reportService.ResendReportByEmailAsync(id, emailRecipients);
                if (!result)
                {
                    return NotFound(new { message = "Report not found" });
                }

                _logger.LogInformation("Report resent: {ReportId} to {Recipients} by user {UserId}", 
                    id, string.Join(", ", emailRecipients), userId);

                return Ok(new { message = "Report sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending report {ReportId}", id);
                return StatusCode(500, new { message = "Error resending report" });
            }
        }

        /// <summary>
        /// Obtiene reportes de un proyecto específico
        /// </summary>
        /// <param name="projectId">ID del proyecto</param>
        /// <returns>Lista de reportes del proyecto</returns>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<ReportDto>>> GetProjectReports(Guid projectId)
        {
            try
            {
                var reports = await _reportService.GetProjectReportsAsync(projectId);
                
                _logger.LogInformation("Retrieved {Count} reports for project {ProjectId}", reports.Count, projectId);
                
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project reports for {ProjectId}", projectId);
                return StatusCode(500, new { message = "Error retrieving project reports" });
            }
        }

        /// <summary>
        /// Genera un reporte rápido (PDF simple)
        /// </summary>
        /// <param name="projectId">ID del proyecto</param>
        /// <param name="reportType">Tipo de reporte</param>
        /// <returns>Archivo PDF del reporte</returns>
        [HttpGet("quick/{projectId}")]
        public async Task<ActionResult> GenerateQuickReport(Guid projectId, [FromQuery] string reportType = "Monthly")
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Generar reporte del último mes
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-1);

                var generateDto = new GenerateReportDto
                {
                    ProjectId = projectId,
                    ReportType = reportType,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    Format = "PDF",
                    Title = $"Reporte Rápido {reportType}",
                    SendByEmail = false
                };

                var report = await _reportService.GenerateReportAsync(generateDto);
                var (fileData, fileName, contentType) = await _reportService.DownloadReportAsync(report.Id);

                _logger.LogInformation("Quick report generated for project {ProjectId} by user {UserId}", projectId, userId);

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quick report for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Error generating quick report" });
            }
        }

        /// <summary>
        /// Obtiene tipos de reportes disponibles
        /// </summary>
        /// <returns>Lista de tipos de reportes</returns>
        [HttpGet("types")]
        public ActionResult<object> GetReportTypes()
        {
            return Ok(new
            {
                reportTypes = ReportTypes.All,
                formats = ReportFormats.All,
                statuses = ReportStatus.All
            });
        }

        /// <summary>
        /// Obtiene estadísticas de reportes del tenant actual
        /// </summary>
        /// <returns>Estadísticas de reportes</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetReportStats()
        {
            try
            {
                // Obtener estadísticas básicas (implementación simplificada)
                var totalReports = await _reportService.GetReportsAsync(1, 1000);
                
                var stats = new
                {
                    totalReports = totalReports.TotalCount,
                    reportsByType = totalReports.Reports
                        .GroupBy(r => r.ReportType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    reportsByStatus = totalReports.Reports
                        .GroupBy(r => r.Status)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    reportsByFormat = totalReports.Reports
                        .GroupBy(r => r.FileFormat)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    recentReports = totalReports.Reports
                        .OrderByDescending(r => r.GeneratedAt)
                        .Take(5)
                        .Select(r => new { r.Id, r.Title, r.GeneratedAt, r.Status })
                        .ToList()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report stats");
                return StatusCode(500, new { message = "Error retrieving report statistics" });
            }
        }

        /// <summary>
        /// Endpoint de prueba para validar autorización en reportes
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
                message = "Report controller authorization successful",
                userId,
                role,
                tenantId,
                canGenerateReports = User.IsInRole(QopiqRoles.CompanyAdmin) || 
                                    User.IsInRole(QopiqRoles.SuperAdmin) || 
                                    User.IsInRole(QopiqRoles.ProjectManager),
                canDeleteReports = User.IsInRole(QopiqRoles.SuperAdmin),
                canResendReports = User.IsInRole(QopiqRoles.CompanyAdmin) || 
                                  User.IsInRole(QopiqRoles.SuperAdmin) || 
                                  User.IsInRole(QopiqRoles.ProjectManager),
                availableFormats = ReportFormats.All,
                availableTypes = ReportTypes.All,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

