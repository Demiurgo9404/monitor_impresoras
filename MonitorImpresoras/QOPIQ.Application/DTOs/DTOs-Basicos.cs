using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Application.DTOs
{
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
        public bool IsActive { get; set; }
    }

    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    // DTOs para Company
    public class CompanyListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateCompanyDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La dirección no puede tener más de 200 caracteres")]
        public string Address { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;
    }

    public class UpdateCompanyDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La dirección no puede tener más de 200 caracteres")]
        public string Address { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;
    }

    // DTOs para Project
    public class CreateProjectDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class UpdateProjectDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string Description { get; set; } = string.Empty;
    }

    public class AssignUserToProjectDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
    }

    // DTOs para Reports
    public class GenerateReportDto
    {
        [Required]
        public string ReportType { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? CompanyId { get; set; }
        public string Format { get; set; } = "PDF";
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    // DTOs para ScheduledReport
    public class ScheduledReportDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty; // CRON expression
        public string Recipients { get; set; } = string.Empty; // Comma-separated emails
        public bool IsActive { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
    }

    public class CreateScheduledReportDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        public string Schedule { get; set; } = string.Empty; // CRON expression

        [Required]
        [EmailAddress]
        public string Recipients { get; set; } = string.Empty; // Comma-separated emails

        public bool IsActive { get; set; } = true;
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    // UserDto movido a AuthDtos.cs

    public class AgentHeartbeatDto
    {
        public string AgentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Metrics { get; set; } = new();
    }

    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
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

