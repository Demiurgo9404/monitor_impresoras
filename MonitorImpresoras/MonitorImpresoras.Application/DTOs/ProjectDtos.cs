using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para mostrar información de proyecto
    /// </summary>
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int MonitoringIntervalMinutes { get; set; }
        public bool EnableRealTimeAlerts { get; set; }
        public bool EnableAutomaticReports { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Estadísticas
        public int TotalPrinters { get; set; }
        public int ActivePrinters { get; set; }
        public int TotalUsers { get; set; }
        public int TotalReports { get; set; }
    }

    /// <summary>
    /// DTO para crear nuevo proyecto
    /// </summary>
    public class CreateProjectDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid CompanyId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(1, 1440)] // 1 minuto a 24 horas
        public int MonitoringIntervalMinutes { get; set; } = 5;

        public bool EnableRealTimeAlerts { get; set; } = true;
        public bool EnableAutomaticReports { get; set; } = true;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar proyecto
    /// </summary>
    public class UpdateProjectDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; } = true;

        [Range(1, 1440)]
        public int MonitoringIntervalMinutes { get; set; }

        public bool EnableRealTimeAlerts { get; set; }
        public bool EnableAutomaticReports { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para lista paginada de proyectos
    /// </summary>
    public class ProjectListDto
    {
        public List<ProjectDto> Projects { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// DTO para estadísticas de proyecto
    /// </summary>
    public class ProjectStatsDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        // Impresoras
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int OfflinePrinters { get; set; }
        public int PrintersWithErrors { get; set; }
        public int PrintersLowToner { get; set; }

        // Actividad
        public long TotalPrintJobs { get; set; }
        public long TotalPagesThisMonth { get; set; }
        public long TotalScansThisMonth { get; set; }
        public long TotalCopiesThisMonth { get; set; }

        // Usuarios y reportes
        public int AssignedUsers { get; set; }
        public int GeneratedReports { get; set; }
        public DateTime? LastReportDate { get; set; }

        // Fechas importantes
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysUntilProjectEnd => EndDate?.Subtract(DateTime.UtcNow).Days;
        public bool IsProjectExpiringSoon => DaysUntilProjectEnd <= 30;
        public bool IsProjectExpired => EndDate <= DateTime.UtcNow;

        // Estado de monitoreo
        public int MonitoringIntervalMinutes { get; set; }
        public bool EnableRealTimeAlerts { get; set; }
        public bool EnableAutomaticReports { get; set; }
        public DateTime? LastMonitoringCheck { get; set; }
    }

    /// <summary>
    /// DTO para asignar usuario a proyecto
    /// </summary>
    public class AssignUserToProjectDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = QopiqRoles.Viewer;

        public bool CanManagePrinters { get; set; } = false;
        public bool CanViewReports { get; set; } = true;
        public bool CanGenerateReports { get; set; } = false;
        public bool CanManageUsers { get; set; } = false;
    }

    /// <summary>
    /// DTO para filtros de búsqueda de proyectos
    /// </summary>
    public class ProjectFiltersDto
    {
        public Guid? CompanyId { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public string? SearchTerm { get; set; }
    }
}
