using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestión de proyectos multi-tenant
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Obtiene todos los proyectos del tenant actual con paginación y filtros
        /// </summary>
        Task<ProjectListDto> GetProjectsAsync(int pageNumber = 1, int pageSize = 10, ProjectFiltersDto? filters = null);

        /// <summary>
        /// Obtiene un proyecto por ID
        /// </summary>
        Task<ProjectDto?> GetProjectByIdAsync(Guid id);

        /// <summary>
        /// Crea un nuevo proyecto
        /// </summary>
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto);

        /// <summary>
        /// Actualiza un proyecto existente
        /// </summary>
        Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto updateDto);

        /// <summary>
        /// Elimina un proyecto (soft delete)
        /// </summary>
        Task<bool> DeleteProjectAsync(Guid id);

        /// <summary>
        /// Obtiene estadísticas de un proyecto
        /// </summary>
        Task<ProjectStatsDto?> GetProjectStatsAsync(Guid id);

        /// <summary>
        /// Obtiene proyectos de una empresa específica
        /// </summary>
        Task<List<ProjectDto>> GetCompanyProjectsAsync(Guid companyId);

        /// <summary>
        /// Obtiene proyectos asignados a un usuario
        /// </summary>
        Task<List<ProjectDto>> GetUserProjectsAsync(string userId);

        /// <summary>
        /// Verifica si el usuario actual tiene acceso al proyecto
        /// </summary>
        Task<bool> HasAccessToProjectAsync(Guid projectId, string userId);

        /// <summary>
        /// Asigna un usuario a un proyecto
        /// </summary>
        Task<bool> AssignUserToProjectAsync(Guid projectId, AssignUserToProjectDto assignDto);

        /// <summary>
        /// Remueve un usuario de un proyecto
        /// </summary>
        Task<bool> RemoveUserFromProjectAsync(Guid projectId, string userId);

        /// <summary>
        /// Obtiene usuarios asignados a un proyecto
        /// </summary>
        Task<List<UserInfoDto>> GetProjectUsersAsync(Guid projectId);
    }
}
