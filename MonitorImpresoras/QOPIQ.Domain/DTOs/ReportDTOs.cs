using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.DTOs
{
    public class ReportDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FilterParameters { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long? FileSize { get; set; }
    }

    public class CreateReportDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PrinterId { get; set; }
        public List<string> ReportSections { get; set; } = new List<string>();
    }

    public class ReportFilterDTO
    {
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PrinterId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ReportSummaryDTO
    {
        public string ReportType { get; set; } = string.Empty;
        public int TotalReports { get; set; }
        public Dictionary<string, int> ReportsByType { get; set; } = new();
        public Dictionary<string, int> ReportsByStatus { get; set; } = new();
        public Dictionary<string, int> ReportsByUser { get; set; } = new();
    }

    // ========== NUEVOS DTOs PARA REPORTES PROGRAMADOS ==========

    /// <summary>
    /// DTO para crear un reporte programado
    /// </summary>
    public class CreateScheduledReportRequestDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public ReportFormat Format { get; set; }

        [Required]
        public ScheduleType ScheduleType { get; set; }

        [Required]
        public ScheduleFrequency Frequency { get; set; }

        // Configuración de horario
        public TimeSpan? StartTime { get; set; }
        public int? DayOfMonth { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public List<int> MonthsOfYear { get; set; } = new();

        public DateTime? NextRunDate { get; set; }

        // Configuración del reporte
        public string ReportConfiguration { get; set; } = string.Empty;

        // Configuración de filtros
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> IncludedPrinters { get; set; } = new();
        public List<string> IncludedDepartments { get; set; } = new();
        public List<string> IncludedUsers { get; set; } = new();

        // Configuración de destinatarios
        public List<string> EmailRecipients { get; set; } = new();
        public List<string> WebhookUrls { get; set; } = new();

        // Configuración de retención
        public int RetentionDays { get; set; } = 30;

        public bool SendOnFailure { get; set; } = true;
    }

    /// <summary>
    /// DTO para actualizar un reporte programado
    /// </summary>
    public class UpdateScheduledReportRequestDTO
    {
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public ReportType? ReportType { get; set; }
        public ReportFormat? Format { get; set; }
        public ScheduleType? ScheduleType { get; set; }
        public ScheduleFrequency? Frequency { get; set; }

        // Configuración de horario
        public TimeSpan? StartTime { get; set; }
        public int? DayOfMonth { get; set; }
        public List<DayOfWeek>? DaysOfWeek { get; set; }
        public List<int>? MonthsOfYear { get; set; }

        public DateTime? NextRunDate { get; set; }
        public bool? IsActive { get; set; }

        // Configuración del reporte
        public string ReportConfiguration { get; set; } = string.Empty;

        // Configuración de filtros
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> IncludedPrinters { get; set; } = new();
        public List<string> IncludedDepartments { get; set; } = new();
        public List<string> IncludedUsers { get; set; } = new();

        // Configuración de destinatarios
        public List<string> EmailRecipients { get; set; } = new();
        public List<string> WebhookUrls { get; set; } = new();

        // Configuración de retención
        public int? RetentionDays { get; set; }
        public bool? SendOnFailure { get; set; }
    }

    /// <summary>
    /// DTO para información de reporte programado
    /// </summary>
    public class ScheduledReportDTO
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public ScheduleFrequency Frequency { get; set; }
        public DateTime NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        // Configuración de filtros
        public DateTime? DateFrom { get; set; } = null;
        public DateTime? DateTo { get; set; } = null;
        public List<string> IncludedPrinters { get; set; } = new();
        public List<string> IncludedDepartments { get; set; } = new();
        public List<string> IncludedUsers { get; set; } = new();

        // Configuración de destinatarios
        public List<string> EmailRecipients { get; set; } = new();
        public List<string> WebhookUrls { get; set; } = new();

        public int RetentionDays { get; set; }
        public bool SendOnFailure { get; set; }

        // Estadísticas
        public int TotalRuns { get; set; }
        public int SuccessfulRuns { get; set; }
        public int FailedRuns { get; set; }
        public string LastError { get; set; } = string.Empty;

        // Información de la última ejecución
        public LastExecutionDTO LastExecution { get; set; } = new();

        // Próximas ejecuciones
        public List<NextExecutionDTO> NextExecutions { get; set; } = new();
    }

    /// <summary>
    /// DTO para información de la última ejecución
    /// </summary>
    public class LastExecutionDTO
    {
        public int ExecutionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ExecutionStatus Status { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long? FileSizeBytes { get; set; }
        public int? RecordCount { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
    }

    /// <summary>
    /// DTO para próximas ejecuciones
    /// </summary>
    public class NextExecutionDTO
    {
        public DateTime ScheduledDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para estadísticas de reportes
    /// </summary>
    public class ReportStatisticsDTO
    {
        public int TotalScheduledReports { get; set; }
        public int ActiveReports { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double SuccessRate { get; set; }
        public Dictionary<ReportType, int> ReportsByType { get; set; } = new();
        public Dictionary<ExecutionStatus, int> ExecutionsByStatus { get; set; } = new();
        public Dictionary<ScheduleFrequency, int> ReportsByFrequency { get; set; } = new();
        public List<ScheduledReportDTO> MostUsedReports { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO para configuración de filtros de reporte
    /// </summary>
    public class ReportFilterConfigurationDTO
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> PrinterIds { get; set; } = new();
        public List<string> DepartmentNames { get; set; } = new();
        public List<string> UserIds { get; set; } = new();
        public bool IncludeInactive { get; set; } = false;
        public bool IncludeDeleted { get; set; } = false;
    }

    /// <summary>
    /// DTO para solicitud de ejecución manual de reporte
    /// </summary>
    public class ManualReportRequestDTO
    {
        [Required]
        public int ScheduledReportId { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> EmailRecipients { get; set; } = new();
        public List<string> WebhookUrls { get; set; } = new();
    }

    /// <summary>
    /// DTO para respuesta de ejecución de reporte
    /// </summary>
    public class ReportExecutionResponseDTO
    {
        public int ExecutionId { get; set; }
        public ExecutionStatus Status { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long? FileSizeBytes { get; set; }
        public int? RecordCount { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> EmailRecipients { get; set; } = new();
        public List<string> WebhookUrls { get; set; } = new();
    }
}

