using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using System.Security.Claims;

namespace QOPIQ.API.Controllers
{
    /// <summary>
    /// Controlador para gestión de empresas multi-tenant
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(
            ICompanyService companyService,
            ITenantAccessor tenantAccessor,
            ILogger<CompanyController> logger)
        {
            _companyService = companyService;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las empresas del tenant actual con paginación
        /// </summary>
        /// <param name="pageNumber">Número de página (default: 1)</param>
        /// <param name="pageSize">Tamaño de página (default: 10)</param>
        /// <param name="searchTerm">Término de búsqueda opcional</param>
        /// <returns>Lista paginada de empresas</returns>
        [HttpGet]
        public async Task<ActionResult<CompanyListDto>> GetCompanies(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _companyService.GetCompaniesAsync(pageNumber, pageSize, searchTerm);
                
                _logger.LogInformation("Retrieved {Count} companies for tenant {TenantId}", 
                    result.Companies.Count, _tenantAccessor.TenantId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving companies");
                return StatusCode(500, new { message = "Error retrieving companies" });
            }
        }

        /// <summary>
        /// Obtiene una empresa específica por ID
        /// </summary>
        /// <param name="id">ID de la empresa</param>
        /// <returns>Información de la empresa</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso a la empresa
                var hasAccess = await _companyService.HasAccessToCompanyAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this company");
                }

                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company {CompanyId}", id);
                return StatusCode(500, new { message = "Error retrieving company" });
            }
        }

        /// <summary>
        /// Crea una nueva empresa (solo Admin)
        /// </summary>
        /// <param name="createDto">Datos de la nueva empresa</param>
        /// <returns>Empresa creada</returns>
        [HttpPost]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin}")]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var company = await _companyService.CreateCompanyAsync(createDto);
                
                _logger.LogInformation("Company created: {CompanyId} by user {UserId}", 
                    company.Id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized company creation attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                return StatusCode(500, new { message = "Error creating company" });
            }
        }

        /// <summary>
        /// Actualiza una empresa existente (solo Admin)
        /// </summary>
        /// <param name="id">ID de la empresa</param>
        /// <param name="updateDto">Datos actualizados</param>
        /// <returns>Empresa actualizada</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin}")]
        public async Task<ActionResult<CompanyDto>> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto updateDto)
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

                // Verificar acceso a la empresa
                var hasAccess = await _companyService.HasAccessToCompanyAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this company");
                }

                var company = await _companyService.UpdateCompanyAsync(id, updateDto);
                if (company == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                _logger.LogInformation("Company updated: {CompanyId} by user {UserId}", id, userId);

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId}", id);
                return StatusCode(500, new { message = "Error updating company" });
            }
        }

        /// <summary>
        /// Elimina una empresa (soft delete, solo SuperAdmin)
        /// </summary>
        /// <param name="id">ID de la empresa</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = QopiqRoles.SuperAdmin)]
        public async Task<ActionResult> DeleteCompany(Guid id)
        {
            try
            {
                var result = await _companyService.DeleteCompanyAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Company not found" });
                }

                _logger.LogInformation("Company deleted: {CompanyId} by user {UserId}", 
                    id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { message = "Company deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {CompanyId}", id);
                return StatusCode(500, new { message = "Error deleting company" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de una empresa
        /// </summary>
        /// <param name="id">ID de la empresa</param>
        /// <returns>Estadísticas de la empresa</returns>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<CompanyStatsDto>> GetCompanyStats(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso a la empresa
                var hasAccess = await _companyService.HasAccessToCompanyAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this company");
                }

                var stats = await _companyService.GetCompanyStatsAsync(id);
                if (stats == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company stats {CompanyId}", id);
                return StatusCode(500, new { message = "Error retrieving company stats" });
            }
        }

        /// <summary>
        /// Obtiene las empresas del usuario actual
        /// </summary>
        /// <returns>Lista de empresas del usuario</returns>
        [HttpGet("my-companies")]
        public async Task<ActionResult<List<CompanyDto>>> GetMyCompanies()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var companies = await _companyService.GetUserCompaniesAsync(userId);
                
                _logger.LogInformation("Retrieved {Count} companies for user {UserId}", companies.Count, userId);
                
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user companies");
                return StatusCode(500, new { message = "Error retrieving user companies" });
            }
        }

        /// <summary>
        /// Endpoint de prueba para validar autorización por roles
        /// </summary>
        /// <returns>Información del contexto de autorización</returns>
        [HttpGet("test-auth")]
        public ActionResult GetAuthTest()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(QopiqClaims.Role)?.Value;
            var tenantId = _tenantAccessor.TenantId;
            var companyId = User.FindFirst(QopiqClaims.CompanyId)?.Value;

            return Ok(new
            {
                message = "Company controller authorization successful",
                userId,
                role,
                tenantId,
                companyId,
                isAdmin = User.IsInRole(QopiqRoles.CompanyAdmin) || User.IsInRole(QopiqRoles.SuperAdmin),
                canCreateCompanies = User.IsInRole(QopiqRoles.CompanyAdmin) || User.IsInRole(QopiqRoles.SuperAdmin),
                canDeleteCompanies = User.IsInRole(QopiqRoles.SuperAdmin),
                timestamp = DateTime.UtcNow
            });
        }
    }
}

