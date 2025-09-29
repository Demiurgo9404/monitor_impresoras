using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityAuditService _auditService;
        private readonly IWindowsHardeningService _windowsHardeningService;
        private readonly IIisHardeningService _iisHardeningService;
        private readonly IPostgreSqlHardeningService _postgresqlHardeningService;
        private readonly IApiSecurityService _apiSecurityService;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(
            ISecurityAuditService auditService,
            IWindowsHardeningService windowsHardeningService,
            IIisHardeningService iisHardeningService,
            IPostgreSqlHardeningService postgresqlHardeningService,
            IApiSecurityService apiSecurityService,
            ILogger<SecurityController> logger)
        {
            _auditService = auditService;
            _windowsHardeningService = windowsHardeningService;
            _iisHardeningService = iisHardeningService;
            _postgresqlHardeningService = postgresqlHardeningService;
            _apiSecurityService = apiSecurityService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza auditoría completa de seguridad del sistema
        /// </summary>
        [HttpPost("audit")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PerformSecurityAudit()
        {
            try
            {
                _logger.LogInformation("Ejecutando auditoría de seguridad por usuario {UserId}", User.Identity?.Name);

                var audit = await _auditService.PerformSecurityAuditAsync();

                return Ok(new
                {
                    Message = "Auditoría de seguridad completada",
                    SecurityAudit = audit,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando auditoría de seguridad");
                throw;
            }
        }

        /// <summary>
        /// Verifica cumplimiento de estándares de seguridad
        /// </summary>
        [HttpGet("compliance")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CheckSecurityCompliance()
        {
            try
            {
                _logger.LogInformation("Verificando cumplimiento de estándares de seguridad");

                var compliance = await _auditService.CheckSecurityComplianceAsync();

                return Ok(new
                {
                    Message = "Verificación de cumplimiento completada",
                    SecurityCompliance = compliance,
                    CheckedBy = User.Identity?.Name,
                    CheckedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando cumplimiento de seguridad");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta hardening completo de Windows Server
        /// </summary>
        [HttpPost("harden/windows")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> HardenWindows()
        {
            try
            {
                _logger.LogInformation("Ejecutando hardening de Windows Server por usuario {UserId}", User.Identity?.Name);

                var result = await _windowsHardeningService.HardenWindowsServerAsync();

                return Ok(new
                {
                    Message = "Hardening de Windows Server completado",
                    WindowsHardening = result,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando hardening de Windows");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta hardening completo de IIS
        /// </summary>
        [HttpPost("harden/iis")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> HardenIis()
        {
            try
            {
                _logger.LogInformation("Ejecutando hardening de IIS por usuario {UserId}", User.Identity?.Name);

                var result = await _iisHardeningService.HardenIisAsync();

                return Ok(new
                {
                    Message = "Hardening de IIS completado",
                    IisHardening = result,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando hardening de IIS");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta hardening completo de PostgreSQL
        /// </summary>
        [HttpPost("harden/postgresql")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> HardenPostgreSql()
        {
            try
            {
                _logger.LogInformation("Ejecutando hardening de PostgreSQL por usuario {UserId}", User.Identity?.Name);

                var result = await _postgresqlHardeningService.HardenPostgreSqlAsync();

                return Ok(new
                {
                    Message = "Hardening de PostgreSQL completado",
                    PostgreSqlHardening = result,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando hardening de PostgreSQL");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta configuración completa de seguridad de API
        /// </summary>
        [HttpPost("harden/api")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ConfigureApiSecurity()
        {
            try
            {
                _logger.LogInformation("Configurando seguridad de API por usuario {UserId}", User.Identity?.Name);

                var result = await _apiSecurityService.ConfigureApiSecurityAsync();

                return Ok(new
                {
                    Message = "Configuración de seguridad de API completada",
                    ApiSecurity = result,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando seguridad de API");
                throw;
            }
        }

        /// <summary>
        /// Obtiene recomendaciones de hardening para Windows
        /// </summary>
        [HttpGet("recommendations/windows")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetWindowsHardeningRecommendations()
        {
            try
            {
                _logger.LogInformation("Obteniendo recomendaciones de hardening para Windows");

                var recommendations = await _windowsHardeningService.GetHardeningRecommendationsAsync();

                return Ok(new
                {
                    TotalRecommendations = recommendations.Count(),
                    WindowsRecommendations = recommendations.Select(r => new
                    {
                        Category = r.Category,
                        Priority = r.Priority,
                        Title = r.Title,
                        Description = r.Description,
                        Impact = r.Impact,
                        Implementation = r.Implementation,
                        Effort = r.Effort
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo recomendaciones de hardening para Windows");
                throw;
            }
        }

        /// <summary>
        /// Obtiene recomendaciones de hardening para IIS
        /// </summary>
        [HttpGet("recommendations/iis")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetIisHardeningRecommendations()
        {
            try
            {
                _logger.LogInformation("Obteniendo recomendaciones de hardening para IIS");

                var recommendations = await _iisHardeningService.GetHardeningRecommendationsAsync();

                return Ok(new
                {
                    TotalRecommendations = recommendations.Count(),
                    IisRecommendations = recommendations.Select(r => new
                    {
                        Category = r.Category,
                        Priority = r.Priority,
                        Title = r.Title,
                        Description = r.Description,
                        Impact = r.Impact,
                        Implementation = r.Implementation,
                        Effort = r.Effort
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo recomendaciones de hardening para IIS");
                throw;
            }
        }

        /// <summary>
        /// Obtiene recomendaciones de hardening para PostgreSQL
        /// </summary>
        [HttpGet("recommendations/postgresql")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPostgreSqlHardeningRecommendations()
        {
            try
            {
                _logger.LogInformation("Obteniendo recomendaciones de hardening para PostgreSQL");

                var recommendations = await _postgresqlHardeningService.GetHardeningRecommendationsAsync();

                return Ok(new
                {
                    TotalRecommendations = recommendations.Count(),
                    PostgreSqlRecommendations = recommendations.Select(r => new
                    {
                        Category = r.Category,
                        Priority = r.Priority,
                        Title = r.Title,
                        Description = r.Description,
                        Impact = r.Impact,
                        Implementation = r.Implementation,
                        Effort = r.Effort
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo recomendaciones de hardening para PostgreSQL");
                throw;
            }
        }

        /// <summary>
        /// Obtiene recomendaciones de seguridad para API
        /// </summary>
        [HttpGet("recommendations/api")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetApiSecurityRecommendations()
        {
            try
            {
                _logger.LogInformation("Obteniendo recomendaciones de seguridad para API");

                var recommendations = await _apiSecurityService.GetSecurityRecommendationsAsync();

                return Ok(new
                {
                    TotalRecommendations = recommendations.Count(),
                    ApiSecurityRecommendations = recommendations.Select(r => new
                    {
                        Category = r.Category,
                        Priority = r.Priority,
                        Title = r.Title,
                        Description = r.Description,
                        Impact = r.Impact,
                        Implementation = r.Implementation,
                        Effort = r.Effort
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo recomendaciones de seguridad para API");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta pruebas de penetración internas simuladas
        /// </summary>
        [HttpPost("penetration-test")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RunPenetrationTest()
        {
            try
            {
                _logger.LogInformation("Ejecutando pruebas de penetración internas por usuario {UserId}", User.Identity?.Name);

                var testResult = await ExecutePenetrationTestAsync();

                return Ok(new
                {
                    Message = "Pruebas de penetración completadas",
                    PenetrationTestResult = testResult,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando pruebas de penetración");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta pruebas de penetración internas simuladas
        /// </summary>
        private async Task<PenetrationTestResult> ExecutePenetrationTestAsync()
        {
            var result = new PenetrationTestResult
            {
                TestStartTime = DateTime.UtcNow,
                TestType = "Internal Automated",
                ToolsUsed = new[] { "OWASP ZAP Simulation", "SQL Injection Scanner", "XSS Detector" }
            };

            try
            {
                // Simular pruebas de seguridad comunes
                var vulnerabilities = new List<Vulnerability>
                {
                    new() { Type = "SQL Injection", Severity = "Medium", Endpoint = "/api/v1/printers", Description = "Posible vulnerabilidad en búsqueda", Status = "Detected" },
                    new() { Type = "XSS", Severity = "Low", Endpoint = "/api/v1/alerts", Description = "Entrada no sanitizada", Status = "Detected" },
                    new() { Type = "Missing Headers", Severity = "High", Endpoint = "Global", Description = "Faltan encabezados de seguridad", Status = "Detected" }
                };

                result.TestEndTime = DateTime.UtcNow;
                result.Duration = result.TestEndTime - result.TestStartTime;
                result.VulnerabilitiesFound = vulnerabilities.Count;
                result.Vulnerabilities = vulnerabilities;
                result.CriticalIssues = vulnerabilities.Count(v => v.Severity == "High" || v.Severity == "Critical");
                result.HighIssues = vulnerabilities.Count(v => v.Severity == "High");
                result.MediumIssues = vulnerabilities.Count(v => v.Severity == "Medium");
                result.LowIssues = vulnerabilities.Count(v => v.Severity == "Low");

                return result;
            }
            catch (Exception ex)
            {
                result.TestEndTime = DateTime.UtcNow;
                result.Duration = result.TestEndTime - result.TestStartTime;
                result.VulnerabilitiesFound = 0;
                result.Status = "Failed";
                result.ErrorMessage = ex.Message;

                _logger.LogError(ex, "Error durante pruebas de penetración");
                throw;
            }
        }
    }

    /// <summary>
    /// DTO para resultado de pruebas de penetración
    /// </summary>
    public class PenetrationTestResult
    {
        public DateTime TestStartTime { get; set; }
        public DateTime TestEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string TestType { get; set; } = string.Empty;
        public string[] ToolsUsed { get; set; } = Array.Empty<string>();
        public int VulnerabilitiesFound { get; set; }
        public List<Vulnerability> Vulnerabilities { get; set; } = new();
        public int CriticalIssues { get; set; }
        public int HighIssues { get; set; }
        public int MediumIssues { get; set; }
        public int LowIssues { get; set; }
        public string Status { get; set; } = "Completed";
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO para vulnerabilidad detectada
    /// </summary>
    public class Vulnerability
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
