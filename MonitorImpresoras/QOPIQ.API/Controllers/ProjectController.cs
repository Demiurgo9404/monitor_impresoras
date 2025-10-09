using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using System.Security.Claims;

namespace QOPIQ.API.Controllers
{
    /// <summary>
    /// Controlador para gestión de proyectos multi-tenant
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectService projectService,
            ITenantAccessor tenantAccessor,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los proyectos del tenant actual con paginación y filtros
        /// </summary>
        /// <param name="pageNumber">Número de página (default: 1)</param>
        /// <param name="pageSize">Tamaño de página (default: 10)</param>
        /// <param name="companyId">Filtrar por empresa</param>
        /// <param name="status">Filtrar por estado</param>
        /// <param name="isActive">Filtrar por activo/inactivo</param>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Lista paginada de proyectos</returns>
        [HttpGet]
        public async Task<ActionResult<ProjectListDto>> GetProjects(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? companyId = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var filters = new ProjectFiltersDto
                {
                    CompanyId = companyId,
                    Status = status,
                    IsActive = isActive,
                    SearchTerm = searchTerm
                };

                var result = await _projectService.GetProjectsAsync(pageNumber, pageSize, filters);
                
                _logger.LogInformation("Retrieved {Count} projects for tenant {TenantId}", 
                    result.Projects.Count, _tenantAccessor.TenantId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return StatusCode(500, new { message = "Error retrieving projects" });
            }
        }

        /// <summary>
        /// Obtiene un proyecto específico por ID
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Información del proyecto</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso al proyecto
                var hasAccess = await _projectService.HasAccessToProjectAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this project");
                }

                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new { message = "Project not found" });
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
                return StatusCode(500, new { message = "Error retrieving project" });
            }
        }

        /// <summary>
        /// Crea un nuevo proyecto (solo Admin y ProjectManager)
        /// </summary>
        /// <param name="createDto">Datos del nuevo proyecto</param>
        /// <returns>Proyecto creado</returns>
        [HttpPost]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var project = await _projectService.CreateProjectAsync(createDto);
                
                _logger.LogInformation("Project created: {ProjectId} by user {UserId}", 
                    project.Id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized project creation attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid project creation data: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { message = "Error creating project" });
            }
        }

        /// <summary>
        /// Actualiza un proyecto existente (solo Admin y ProjectManager)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <param name="updateDto">Datos actualizados</param>
        /// <returns>Proyecto actualizado</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto updateDto)
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

                // Verificar acceso al proyecto
                var hasAccess = await _projectService.HasAccessToProjectAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this project");
                }

                var project = await _projectService.UpdateProjectAsync(id, updateDto);
                if (project == null)
                {
                    return NotFound(new { message = "Project not found" });
                }

                _logger.LogInformation("Project updated: {ProjectId} by user {UserId}", id, userId);

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, new { message = "Error updating project" });
            }
        }

        /// <summary>
        /// Elimina un proyecto (soft delete, solo SuperAdmin)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = QopiqRoles.SuperAdmin)]
        public async Task<ActionResult> DeleteProject(Guid id)
        {
            try
            {
                var result = await _projectService.DeleteProjectAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Project not found" });
                }

                _logger.LogInformation("Project deleted: {ProjectId} by user {UserId}", 
                    id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                return StatusCode(500, new { message = "Error deleting project" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de un proyecto
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Estadísticas del proyecto</returns>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ProjectStatsDto>> GetProjectStats(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso al proyecto
                var hasAccess = await _projectService.HasAccessToProjectAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this project");
                }

                var stats = await _projectService.GetProjectStatsAsync(id);
                if (stats == null)
                {
                    return NotFound(new { message = "Project not found" });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project stats {ProjectId}", id);
                return StatusCode(500, new { message = "Error retrieving project stats" });
            }
        }

        /// <summary>
        /// Obtiene proyectos de una empresa específica
        /// </summary>
        /// <param name="companyId">ID de la empresa</param>
        /// <returns>Lista de proyectos de la empresa</returns>
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<List<ProjectDto>>> GetCompanyProjects(Guid companyId)
        {
            try
            {
                var projects = await _projectService.GetCompanyProjectsAsync(companyId);
                
                _logger.LogInformation("Retrieved {Count} projects for company {CompanyId}", projects.Count, companyId);
                
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company projects for company {CompanyId}", companyId);
                return StatusCode(500, new { message = "Error retrieving company projects" });
            }
        }

        /// <summary>
        /// Obtiene los proyectos del usuario actual
        /// </summary>
        /// <returns>Lista de proyectos del usuario</returns>
        [HttpGet("my-projects")]
        public async Task<ActionResult<List<ProjectDto>>> GetMyProjects()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var projects = await _projectService.GetUserProjectsAsync(userId);
                
                _logger.LogInformation("Retrieved {Count} projects for user {UserId}", projects.Count, userId);
                
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user projects");
                return StatusCode(500, new { message = "Error retrieving user projects" });
            }
        }

        /// <summary>
        /// Asigna un usuario a un proyecto (solo Admin y ProjectManager)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <param name="assignDto">Datos de asignación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/users")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult> AssignUserToProject(Guid id, [FromBody] AssignUserToProjectDto assignDto)
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

                // Verificar acceso al proyecto
                var hasAccess = await _projectService.HasAccessToProjectAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this project");
                }

                var result = await _projectService.AssignUserToProjectAsync(id, assignDto);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to assign user to project" });
                }

                _logger.LogInformation("User {AssignedUserId} assigned to project {ProjectId} by user {UserId}", 
                    assignDto.UserId, id, userId);

                return Ok(new { message = "User assigned to project successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user to project {ProjectId}", id);
                return StatusCode(500, new { message = "Error assigning user to project" });
            }
        }

        /// <summary>
        /// Remueve un usuario de un proyecto (solo Admin y ProjectManager)
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}/users/{userId}")]
        [Authorize(Roles = $"{QopiqRoles.SuperAdmin},{QopiqRoles.CompanyAdmin},{QopiqRoles.ProjectManager}")]
        public async Task<ActionResult> RemoveUserFromProject(Guid id, string userId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso al proyecto
                var hasAccess = await _projectService.HasAccessToProjectAsync(id, currentUserId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this project");
                }

                var result = await _projectService.RemoveUserFromProjectAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = "User assignment not found" });
                }

                _logger.LogInformation("User {RemovedUserId} removed from project {ProjectId} by user {UserId}", 
                    userId, id, currentUserId);

                return Ok(new { message = "User removed from project successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from project {ProjectId}", id);
                return StatusCode(500, new { message = "Error removing user from project" });
            }
        }

        /// <summary>
        /// Obtiene usuarios asignados a un proyecto
        /// </summary>
        /// <param name="id">ID del proyecto</param>
        /// <returns>Lista de usuarios del proyecto</returns>
        [HttpGet("{id}/users")]
        public async Task<ActionResult<List<UserInfoDto>>> GetProjectUsers(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Verificar acceso al proyecto
                var hasAccess = await _projectService.HasAccessToProjectAsync(id, userId);
                if (!hasAccess)
                {
                    return Forbid("Access denied to this project");
                }

                var users = await _projectService.GetProjectUsersAsync(id);
                
                _logger.LogInformation("Retrieved {Count} users for project {ProjectId}", users.Count, id);
                
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project users for project {ProjectId}", id);
                return StatusCode(500, new { message = "Error retrieving project users" });
            }
        }

        /// <summary>
        /// Endpoint de prueba para validar autorización por roles en proyectos
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
                message = "Project controller authorization successful",
                userId,
                role,
                tenantId,
                companyId,
                isAdmin = User.IsInRole(QopiqRoles.CompanyAdmin) || User.IsInRole(QopiqRoles.SuperAdmin),
                canCreateProjects = User.IsInRole(QopiqRoles.CompanyAdmin) || 
                                   User.IsInRole(QopiqRoles.SuperAdmin) || 
                                   User.IsInRole(QopiqRoles.ProjectManager),
                canDeleteProjects = User.IsInRole(QopiqRoles.SuperAdmin),
                canManageUsers = User.IsInRole(QopiqRoles.CompanyAdmin) || 
                                User.IsInRole(QopiqRoles.SuperAdmin) || 
                                User.IsInRole(QopiqRoles.ProjectManager),
                timestamp = DateTime.UtcNow
            });
        }
    }
}

