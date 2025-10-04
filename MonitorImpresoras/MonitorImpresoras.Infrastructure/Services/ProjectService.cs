using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de proyectos multi-tenant
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(
            ApplicationDbContext context,
            ITenantAccessor tenantAccessor,
            ILogger<ProjectService> logger)
        {
            _context = context;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        public async Task<ProjectListDto> GetProjectsAsync(int pageNumber = 1, int pageSize = 10, ProjectFiltersDto? filters = null)
        {
            try
            {
                var query = _context.Projects
                    .Include(p => p.Company)
                    .AsQueryable();

                // Aplicar filtros
                if (filters != null)
                {
                    if (filters.CompanyId.HasValue)
                        query = query.Where(p => p.CompanyId == filters.CompanyId.Value);

                    if (!string.IsNullOrWhiteSpace(filters.Status))
                        query = query.Where(p => p.Status == filters.Status);

                    if (filters.IsActive.HasValue)
                        query = query.Where(p => p.IsActive == filters.IsActive.Value);

                    if (filters.StartDateFrom.HasValue)
                        query = query.Where(p => p.StartDate >= filters.StartDateFrom.Value);

                    if (filters.StartDateTo.HasValue)
                        query = query.Where(p => p.StartDate <= filters.StartDateTo.Value);

                    if (filters.EndDateFrom.HasValue)
                        query = query.Where(p => p.EndDate >= filters.EndDateFrom.Value);

                    if (filters.EndDateTo.HasValue)
                        query = query.Where(p => p.EndDate <= filters.EndDateTo.Value);

                    if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
                    {
                        query = query.Where(p =>
                            p.Name.Contains(filters.SearchTerm) ||
                            p.ClientName.Contains(filters.SearchTerm) ||
                            p.Description.Contains(filters.SearchTerm) ||
                            p.City.Contains(filters.SearchTerm));
                    }
                }

                var totalCount = await query.CountAsync();

                var projects = await query
                    .OrderBy(p => p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CompanyId = p.CompanyId,
                        CompanyName = p.Company.Name,
                        Description = p.Description,
                        ClientName = p.ClientName,
                        Address = p.Address,
                        City = p.City,
                        State = p.State,
                        PostalCode = p.PostalCode,
                        ContactPerson = p.ContactPerson,
                        ContactPhone = p.ContactPhone,
                        ContactEmail = p.ContactEmail,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Status = p.Status,
                        IsActive = p.IsActive,
                        MonitoringIntervalMinutes = p.MonitoringIntervalMinutes,
                        EnableRealTimeAlerts = p.EnableRealTimeAlerts,
                        EnableAutomaticReports = p.EnableAutomaticReports,
                        Notes = p.Notes,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        TotalPrinters = p.Printers.Count,
                        ActivePrinters = p.Printers.Count(pr => pr.IsOnline),
                        TotalUsers = p.ProjectUsers.Count,
                        TotalReports = p.Reports.Count
                    })
                    .ToListAsync();

                return new ProjectListDto
                {
                    Projects = projects,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                throw;
            }
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Company)
                    .Include(p => p.Printers)
                    .Include(p => p.ProjectUsers)
                    .Include(p => p.Reports)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return null;
                }

                return new ProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    CompanyId = project.CompanyId,
                    CompanyName = project.Company.Name,
                    Description = project.Description,
                    ClientName = project.ClientName,
                    Address = project.Address,
                    City = project.City,
                    State = project.State,
                    PostalCode = project.PostalCode,
                    ContactPerson = project.ContactPerson,
                    ContactPhone = project.ContactPhone,
                    ContactEmail = project.ContactEmail,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Status = project.Status,
                    IsActive = project.IsActive,
                    MonitoringIntervalMinutes = project.MonitoringIntervalMinutes,
                    EnableRealTimeAlerts = project.EnableRealTimeAlerts,
                    EnableAutomaticReports = project.EnableAutomaticReports,
                    Notes = project.Notes,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt,
                    TotalPrinters = project.Printers.Count,
                    ActivePrinters = project.Printers.Count(p => p.IsOnline),
                    TotalUsers = project.ProjectUsers.Count,
                    TotalReports = project.Reports.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
                throw;
            }
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new UnauthorizedAccessException("No tenant context available");
                }

                // Verificar que la empresa pertenece al tenant actual
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Id == createDto.CompanyId && c.TenantId == tenantId);

                if (company == null)
                {
                    throw new ArgumentException("Company not found or access denied");
                }

                var project = new Project
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CompanyId = createDto.CompanyId,
                    Name = createDto.Name,
                    Description = createDto.Description,
                    ClientName = createDto.ClientName,
                    Address = createDto.Address,
                    City = createDto.City,
                    State = createDto.State,
                    PostalCode = createDto.PostalCode,
                    ContactPerson = createDto.ContactPerson,
                    ContactPhone = createDto.ContactPhone,
                    ContactEmail = createDto.ContactEmail,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                    Status = "Active",
                    IsActive = true,
                    MonitoringIntervalMinutes = createDto.MonitoringIntervalMinutes,
                    EnableRealTimeAlerts = createDto.EnableRealTimeAlerts,
                    EnableAutomaticReports = createDto.EnableAutomaticReports,
                    Notes = createDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Project created successfully: {ProjectId} in tenant {TenantId}", project.Id, tenantId);

                return new ProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    CompanyId = project.CompanyId,
                    CompanyName = company.Name,
                    Description = project.Description,
                    ClientName = project.ClientName,
                    Address = project.Address,
                    City = project.City,
                    State = project.State,
                    PostalCode = project.PostalCode,
                    ContactPerson = project.ContactPerson,
                    ContactPhone = project.ContactPhone,
                    ContactEmail = project.ContactEmail,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Status = project.Status,
                    IsActive = project.IsActive,
                    MonitoringIntervalMinutes = project.MonitoringIntervalMinutes,
                    EnableRealTimeAlerts = project.EnableRealTimeAlerts,
                    EnableAutomaticReports = project.EnableAutomaticReports,
                    Notes = project.Notes,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt,
                    TotalPrinters = 0,
                    ActivePrinters = 0,
                    TotalUsers = 0,
                    TotalReports = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                throw;
            }
        }

        public async Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto updateDto)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Company)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return null;
                }

                // Actualizar propiedades
                project.Name = updateDto.Name;
                project.Description = updateDto.Description;
                project.ClientName = updateDto.ClientName;
                project.Address = updateDto.Address;
                project.City = updateDto.City;
                project.State = updateDto.State;
                project.PostalCode = updateDto.PostalCode;
                project.ContactPerson = updateDto.ContactPerson;
                project.ContactPhone = updateDto.ContactPhone;
                project.ContactEmail = updateDto.ContactEmail;
                project.StartDate = updateDto.StartDate;
                project.EndDate = updateDto.EndDate;
                project.Status = updateDto.Status;
                project.IsActive = updateDto.IsActive;
                project.MonitoringIntervalMinutes = updateDto.MonitoringIntervalMinutes;
                project.EnableRealTimeAlerts = updateDto.EnableRealTimeAlerts;
                project.EnableAutomaticReports = updateDto.EnableAutomaticReports;
                project.Notes = updateDto.Notes;
                project.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Project updated successfully: {ProjectId}", id);

                return await GetProjectByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
                if (project == null)
                {
                    return false;
                }

                // Soft delete
                project.IsActive = false;
                project.Status = "Cancelled";
                project.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Project soft deleted: {ProjectId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                throw;
            }
        }

        public async Task<ProjectStatsDto?> GetProjectStatsAsync(Guid id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Company)
                    .Include(p => p.Printers)
                    .Include(p => p.ProjectUsers)
                    .Include(p => p.Reports)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return null;
                }

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                return new ProjectStatsDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    CompanyName = project.Company.Name,
                    TotalPrinters = project.Printers.Count,
                    OnlinePrinters = project.Printers.Count(p => p.IsOnline),
                    OfflinePrinters = project.Printers.Count(p => !p.IsOnline),
                    PrintersWithErrors = project.Printers.Count(p => p.NeedsUserAttention),
                    PrintersLowToner = project.Printers.Count(p => p.LowTonerWarning),
                    TotalPrintJobs = project.Printers.Sum(p => p.TotalPagesPrinted ?? 0),
                    TotalPagesThisMonth = project.Printers
                        .Where(p => p.LastChecked?.Month == currentMonth && p.LastChecked?.Year == currentYear)
                        .Sum(p => p.TotalPagesPrinted ?? 0),
                    TotalScansThisMonth = project.Printers
                        .Where(p => p.LastChecked?.Month == currentMonth && p.LastChecked?.Year == currentYear)
                        .Sum(p => p.TotalScans ?? 0),
                    TotalCopiesThisMonth = project.Printers
                        .Where(p => p.LastChecked?.Month == currentMonth && p.LastChecked?.Year == currentYear)
                        .Sum(p => p.TotalCopies ?? 0),
                    AssignedUsers = project.ProjectUsers.Count(pu => pu.IsActive),
                    GeneratedReports = project.Reports.Count,
                    LastReportDate = project.Reports.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.CreatedAt,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    MonitoringIntervalMinutes = project.MonitoringIntervalMinutes,
                    EnableRealTimeAlerts = project.EnableRealTimeAlerts,
                    EnableAutomaticReports = project.EnableAutomaticReports,
                    LastMonitoringCheck = project.Printers.Max(p => p.LastChecked)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project stats {ProjectId}", id);
                throw;
            }
        }

        public async Task<List<ProjectDto>> GetCompanyProjectsAsync(Guid companyId)
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Company)
                    .Where(p => p.CompanyId == companyId && p.IsActive)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CompanyId = p.CompanyId,
                        CompanyName = p.Company.Name,
                        Description = p.Description,
                        ClientName = p.ClientName,
                        Address = p.Address,
                        City = p.City,
                        State = p.State,
                        PostalCode = p.PostalCode,
                        ContactPerson = p.ContactPerson,
                        ContactPhone = p.ContactPhone,
                        ContactEmail = p.ContactEmail,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Status = p.Status,
                        IsActive = p.IsActive,
                        MonitoringIntervalMinutes = p.MonitoringIntervalMinutes,
                        EnableRealTimeAlerts = p.EnableRealTimeAlerts,
                        EnableAutomaticReports = p.EnableAutomaticReports,
                        Notes = p.Notes,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        TotalPrinters = p.Printers.Count,
                        ActivePrinters = p.Printers.Count(pr => pr.IsOnline),
                        TotalUsers = p.ProjectUsers.Count,
                        TotalReports = p.Reports.Count
                    })
                    .ToListAsync();

                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company projects for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<ProjectDto>> GetUserProjectsAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return new List<ProjectDto>();
                }

                IQueryable<Project> query = _context.Projects.Include(p => p.Company);

                // Si es SuperAdmin, puede ver todos los proyectos del tenant
                if (user.Role == QopiqRoles.SuperAdmin)
                {
                    // Ya filtrado por tenant automáticamente
                }
                // Si es CompanyAdmin, puede ver todos los proyectos de su empresa
                else if (user.Role == QopiqRoles.CompanyAdmin && user.CompanyId.HasValue)
                {
                    query = query.Where(p => p.CompanyId == user.CompanyId.Value);
                }
                // Para otros roles, solo proyectos asignados
                else
                {
                    query = query.Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId && pu.IsActive));
                }

                var projects = await query
                    .Where(p => p.IsActive)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CompanyId = p.CompanyId,
                        CompanyName = p.Company.Name,
                        Description = p.Description,
                        ClientName = p.ClientName,
                        Address = p.Address,
                        City = p.City,
                        State = p.State,
                        PostalCode = p.PostalCode,
                        ContactPerson = p.ContactPerson,
                        ContactPhone = p.ContactPhone,
                        ContactEmail = p.ContactEmail,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Status = p.Status,
                        IsActive = p.IsActive,
                        MonitoringIntervalMinutes = p.MonitoringIntervalMinutes,
                        EnableRealTimeAlerts = p.EnableRealTimeAlerts,
                        EnableAutomaticReports = p.EnableAutomaticReports,
                        Notes = p.Notes,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        TotalPrinters = p.Printers.Count,
                        ActivePrinters = p.Printers.Count(pr => pr.IsOnline),
                        TotalUsers = p.ProjectUsers.Count,
                        TotalReports = p.Reports.Count
                    })
                    .ToListAsync();

                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user projects for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> HasAccessToProjectAsync(Guid projectId, string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return false;
                }

                // SuperAdmin tiene acceso a todo
                if (user.Role == QopiqRoles.SuperAdmin)
                {
                    return true;
                }

                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null)
                {
                    return false;
                }

                // CompanyAdmin tiene acceso a proyectos de su empresa
                if (user.Role == QopiqRoles.CompanyAdmin && user.CompanyId == project.CompanyId)
                {
                    return true;
                }

                // Verificar asignación directa al proyecto
                var projectUser = await _context.ProjectUsers
                    .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId && pu.IsActive);

                return projectUser != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking project access for user {UserId} and project {ProjectId}", userId, projectId);
                return false;
            }
        }

        public async Task<bool> AssignUserToProjectAsync(Guid projectId, AssignUserToProjectDto assignDto)
        {
            try
            {
                // Verificar que el proyecto existe
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null)
                {
                    return false;
                }

                // Verificar que el usuario existe y pertenece al mismo tenant
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == assignDto.UserId && u.TenantId == project.TenantId);
                if (user == null)
                {
                    return false;
                }

                // Verificar si ya está asignado
                var existingAssignment = await _context.ProjectUsers
                    .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == assignDto.UserId);

                if (existingAssignment != null)
                {
                    // Actualizar asignación existente
                    existingAssignment.Role = assignDto.Role;
                    existingAssignment.CanManagePrinters = assignDto.CanManagePrinters;
                    existingAssignment.CanViewReports = assignDto.CanViewReports;
                    existingAssignment.CanGenerateReports = assignDto.CanGenerateReports;
                    existingAssignment.CanManageUsers = assignDto.CanManageUsers;
                    existingAssignment.IsActive = true;
                    existingAssignment.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Crear nueva asignación
                    var projectUser = new ProjectUser
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        UserId = assignDto.UserId,
                        Role = assignDto.Role,
                        CanManagePrinters = assignDto.CanManagePrinters,
                        CanViewReports = assignDto.CanViewReports,
                        CanGenerateReports = assignDto.CanGenerateReports,
                        CanManageUsers = assignDto.CanManageUsers,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.ProjectUsers.Add(projectUser);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} assigned to project {ProjectId} with role {Role}", 
                    assignDto.UserId, projectId, assignDto.Role);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user {UserId} to project {ProjectId}", assignDto.UserId, projectId);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromProjectAsync(Guid projectId, string userId)
        {
            try
            {
                var projectUser = await _context.ProjectUsers
                    .FirstOrDefaultAsync(pu => pu.ProjectId == projectId && pu.UserId == userId);

                if (projectUser == null)
                {
                    return false;
                }

                // Soft delete
                projectUser.IsActive = false;
                projectUser.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} removed from project {ProjectId}", userId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from project {ProjectId}", userId, projectId);
                return false;
            }
        }

        public async Task<List<UserInfoDto>> GetProjectUsersAsync(Guid projectId)
        {
            try
            {
                var users = await _context.ProjectUsers
                    .Include(pu => pu.User)
                    .ThenInclude(u => u.Company)
                    .Where(pu => pu.ProjectId == projectId && pu.IsActive)
                    .Select(pu => new UserInfoDto
                    {
                        Id = pu.User.Id,
                        Email = pu.User.Email ?? "",
                        FirstName = pu.User.FirstName,
                        LastName = pu.User.LastName,
                        FullName = pu.User.GetFullName(),
                        Role = pu.Role,
                        CompanyId = pu.User.CompanyId,
                        CompanyName = pu.User.Company != null ? pu.User.Company.Name : null,
                        Permissions = new[] { 
                            pu.CanManagePrinters ? "manage:printers" : "read:printers",
                            pu.CanViewReports ? "read:reports" : "",
                            pu.CanGenerateReports ? "write:reports" : "",
                            pu.CanManageUsers ? "manage:users" : ""
                        }.Where(p => !string.IsNullOrEmpty(p)).ToArray()
                    })
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project users for project {ProjectId}", projectId);
                throw;
            }
        }
    }
}
