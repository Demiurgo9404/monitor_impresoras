using System;
using System.Collections.Generic;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Domain.DTOs
{
    /// <summary>
    /// DTO para reportes programados
    /// </summary>
    public class ScheduledReportDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public int Frequency { get; set; }
        public TimeSpan StartTime { get; set; }
        public int? DayOfMonth { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
        public string MonthsOfYear { get; set; } = string.Empty;
        public DateTime NextRunDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;
        public int TotalRuns { get; set; }
        public int SuccessfulRuns { get; set; }
        public int FailedRuns { get; set; }
        public DateTime? LastRunDate { get; set; }
        public List<string> EmailRecipients { get; set; } = new();
        public string WebhookUrl { get; set; } = string.Empty;
        public ReportConfigurationDto Configuration { get; set; } = new();
    }

    /// <summary>
    /// DTO para crear reportes programados
    /// </summary>
    public class CreateScheduledReportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public int Frequency { get; set; }
        public TimeSpan StartTime { get; set; }
        public int? DayOfMonth { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
        public string MonthsOfYear { get; set; } = string.Empty;
        public DateTime NextRunDate { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string> EmailRecipients { get; set; } = new();
        public string WebhookUrl { get; set; } = string.Empty;
        public ReportConfigurationDto Configuration { get; set; } = new();
    }

    /// <summary>
    /// DTO para actualizar reportes programados
    /// </summary>
    public class UpdateScheduledReportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public int Frequency { get; set; }
        public TimeSpan StartTime { get; set; }
        public int? DayOfMonth { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
        public string MonthsOfYear { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> EmailRecipients { get; set; } = new();
        public string WebhookUrl { get; set; } = string.Empty;
        public ReportConfigurationDto Configuration { get; set; } = new();
    }

    /// <summary>
    /// Configuración del reporte
    /// </summary>
    public class ReportConfigurationDto
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> IncludedPrinters { get; set; } = new();
        public List<string> IncludedDepartments { get; set; } = new();
        public List<string> IncludedUsers { get; set; } = new();
        public Dictionary<string, object> CustomParameters { get; set; } = new();
    }

    /// <summary>
    /// Resultado de ejecución de reporte
    /// </summary>
    public class ReportExecutionResult
    {
        public bool Success { get; set; }
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public ReportFormat Format { get; set; }
        public int Rows { get; set; }
        public TimeSpan Duration { get; set; }
        public AlertAnalysisResult Analysis { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public List<Dictionary<string, object>> ReportData { get; set; } = new();
    }

    /// <summary>
    /// DTO para información de tenant
    /// </summary>
    public class TenantInfo
    {
        public Guid Id { get; set; }
        public string TenantKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public SubscriptionTier Tier { get; set; }
    }

    /// <summary>
    /// Configuración SMTP
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }
}
