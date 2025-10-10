using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Application.DTOs
{
    // DTOs básicos para compilación
    public class PrinterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
    }

    public class CreatePrinterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string IpAddress { get; set; } = string.Empty;
        
        public string Location { get; set; } = string.Empty;
    }

    public class UpdatePrinterDto
    {
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class PrinterStatusDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // DTOs para Company
    public class CompanyListDto
    {
        public List<CompanyDto> Companies { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateCompanyDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string TaxId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class UpdateCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    // DTOs para Project
    public class CreateProjectDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public Guid CompanyId { get; set; }
        
        public string ClientName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }

    public class UpdateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }

    public class AssignUserToProjectDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string Role { get; set; } = string.Empty;
    }

    // DTOs para Reports
    public class GenerateReportDto
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        public string ReportType { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "PDF";
    }

    // DTOs para ScheduledReport
    public class ScheduledReportDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateScheduledReportDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        public string ReportType { get; set; } = string.Empty;
        
        [Required]
        public string CronExpression { get; set; } = string.Empty;
        
        public string Format { get; set; } = "PDF";
        public List<string> Recipients { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    // UserDto movido a AuthDtos.cs

    public class AgentHeartbeatDto
    {
        public string AgentId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TenantDto
    {
        public Guid Id { get; set; }
        public string TenantKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int OfflinePrinters { get; set; }
        public int TotalReports { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalCompanies { get; set; }
    }
}

