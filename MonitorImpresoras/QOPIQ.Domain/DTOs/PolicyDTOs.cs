using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.DTOs
{
    /// <summary>
    /// Solicitud para evaluar políticas de impresión
    /// </summary>
    public class PolicyEvaluationRequestDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        public int Pages { get; set; }

        [Required]
        public bool IsColor { get; set; }

        [Required]
        public bool IsDuplex { get; set; }

        [Required]
        [StringLength(50)]
        public string PaperSize { get; set; } = string.Empty;

        [StringLength(100)]
        public string PaperType { get; set; } = string.Empty;

        [Required]
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string PrinterId { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado de la evaluación de políticas
    /// </summary>
    public class PolicyEvaluationResultDTO
    {
        public bool IsAllowed { get; set; } = false;
        public List<AppliedPolicyDTO> AppliedPolicies { get; set; } = new();
        public List<PolicyViolationDTO> Violations { get; set; } = new();
        public List<PolicyWarningDTO> Warnings { get; set; } = new();
        public List<RequiredApprovalDTO> RequiredApprovals { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public PolicySeverity Severity { get; set; } = PolicySeverity.Low;
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Política aplicada durante la evaluación
    /// </summary>
    public class AppliedPolicyDTO
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PolicyAction Action { get; set; }
        public string AppliedRule { get; set; } = string.Empty;
        public decimal? ModifiedValue { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Violación de política detectada
    /// </summary>
    public class PolicyViolationDTO
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PolicySeverity Severity { get; set; }
        public bool BlocksPrinting { get; set; }
    }

    /// <summary>
    /// Advertencia de política
    /// </summary>
    public class PolicyWarningDTO
    {
        public string WarningType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public PolicySeverity Severity { get; set; } = PolicySeverity.Low;
    }

    /// <summary>
    /// Aprobación requerida para proceder
    /// </summary>
    public class RequiredApprovalDTO
    {
        public string ApprovalType { get; set; } = string.Empty;
        public string ApproverRole { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public TimeSpan? Timeout { get; set; }
    }

    /// <summary>
    /// Solicitud para crear una nueva política
    /// </summary>
    public class CreatePolicyRequestDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public PolicyType PolicyType { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public int Priority { get; set; } = 100;

        public PolicyConditionDTO Conditions { get; set; } = new();

        public PolicyActionDTO Actions { get; set; } = new();

        public PolicyScheduleDTO Schedule { get; set; } = new();

        public PolicyLimitsDTO Limits { get; set; } = new();

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Solicitud para actualizar una política
    /// </summary>
    public class UpdatePolicyRequestDTO
    {
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public PolicyType? PolicyType { get; set; } = null;

        public bool? IsActive { get; set; } = null;

        public int? Priority { get; set; } = null;

        public PolicyConditionDTO Conditions { get; set; } = new();

        public PolicyActionDTO Actions { get; set; } = new();

        public PolicyScheduleDTO Schedule { get; set; } = new();

        public PolicyLimitsDTO Limits { get; set; } = new();
    }

    /// <summary>
    /// Solicitud para cambiar el estado de una política
    /// </summary>
    public class PolicyStatusRequestDTO
    {
        [Required]
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Condiciones para aplicar una política
    /// </summary>
    public class PolicyConditionDTO
    {
        public List<string> UserRoles { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public List<string> Users { get; set; } = new();
        public List<string> Printers { get; set; } = new();
        public List<string> PaperSizes { get; set; } = new();
        public List<string> PaperTypes { get; set; } = new();
        public int? MinPages { get; set; }
        public int? MaxPages { get; set; }
        public bool? AllowColor { get; set; }
        public bool? AllowDuplex { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<DayOfWeek> AllowedDays { get; set; } = new();
    }

    /// <summary>
    /// Acciones que realiza una política
    /// </summary>
    public class PolicyActionDTO
    {
        public PolicyAction DefaultAction { get; set; } = PolicyAction.Allow;
        public bool RequireApproval { get; set; }
        public string ApproverRole { get; set; } = string.Empty;
        public int? CostMultiplier { get; set; }
        public int? CostAddition { get; set; }
        public string CustomMessage { get; set; } = string.Empty;
        public List<string> Notifications { get; set; } = new();
        public string RedirectAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Horario de aplicación de la política
    /// </summary>
    public class PolicyScheduleDTO
    {
        public bool UseSchedule { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<DayOfWeek> ActiveDays { get; set; } = new();
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Límites de la política
    /// </summary>
    public class PolicyLimitsDTO
    {
        public int? DailyLimit { get; set; }
        public int? WeeklyLimit { get; set; }
        public int? MonthlyLimit { get; set; }
        public int? YearlyLimit { get; set; }
        public int? ConsecutiveLimit { get; set; }
        public TimeSpan? CooldownPeriod { get; set; }
        public int? MaxCostPerJob { get; set; }
    }

    /// <summary>
    /// Estadísticas de aplicación de políticas
    /// </summary>
    public class PolicyStatisticsDTO
    {
        public int TotalEvaluations { get; set; }
        public int AllowedPrints { get; set; }
        public int BlockedPrints { get; set; }
        public int RequiredApprovals { get; set; }
        public int WarningsIssued { get; set; }
        public Dictionary<int, int> PolicyUsage { get; set; } = new();
        public Dictionary<string, int> TopViolations { get; set; } = new();
        public Dictionary<string, int> DepartmentUsage { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Tipos de políticas disponibles
    /// </summary>
    public enum PolicyType
    {
        PageLimit,
        ColorRestriction,
        TimeRestriction,
        CostControl,
        DepartmentQuota,
        UserQuota,
        PrinterAccess,
        PaperRestriction,
        ApprovalRequired,
        QualityControl,
        SecurityPolicy,
        EnvironmentalPolicy,
        Custom
    }

    /// <summary>
    /// Acciones que puede tomar una política
    /// </summary>
    public enum PolicyAction
    {
        Allow,
        Block,
        Warn,
        RequireApproval,
        ModifyCost,
        Redirect,
        LogOnly
    }

    /// <summary>
    /// Severidad de una violación o advertencia
    /// </summary>
    public enum PolicySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}

