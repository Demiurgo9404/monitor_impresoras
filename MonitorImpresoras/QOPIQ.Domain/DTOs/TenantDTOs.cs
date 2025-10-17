using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using SubscriptionStatusEnum = QOPIQ.Domain.Enums.SubscriptionStatus;
using PrinterStatusEnum = QOPIQ.Domain.Enums.PrinterStatus;
using InvoiceStatus = QOPIQ.Domain.Enums.InvoiceStatus;

namespace QOPIQ.Domain.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo tenant
    /// </summary>
    public class CreateTenantRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string AdminEmail { get; set; } = string.Empty;

        [StringLength(50)]
        public string Timezone { get; set; } = "America/Mexico_City";

        [StringLength(3)]
        public string Currency { get; set; } = "MXN";

        [StringLength(500)]
        public string LogoUrl { get; set; } = string.Empty;

        // Configuración de base de datos
        [StringLength(100)]
        public string DatabaseName { get; set; } = string.Empty;

        [StringLength(500)]
        public string ConnectionString { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para información de tenant
    /// </summary>
    public class TenantDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAccess { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        // Límites actuales
        public int MaxPrinters { get; set; }
        public int MaxUsers { get; set; }
        public int MaxPolicies { get; set; }
        public long MaxStorageBytes { get; set; }

        // Uso actual
        public int CurrentPrinters { get; set; }
        public int CurrentUsers { get; set; }
        public long CurrentStorageBytes { get; set; }

        // Suscripción actual
        public CurrentSubscriptionDTO CurrentSubscription { get; set; } = new();
    }

    /// <summary>
    /// DTO para suscripción actual del tenant
    /// </summary>
    public class CurrentSubscriptionDTO
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public SubscriptionStatusEnum Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsTrial { get; set; }
        public int DaysRemaining { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public BillingCycle BillingCycle { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public List<PaymentDTO> RecentPayments { get; set; } = new();
    }

    /// <summary>
    /// DTO para planes de suscripción
    /// </summary>
    public class PlanDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public PlanType Type { get; set; }
        public int TrialDays { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public string Currency { get; set; } = string.Empty;

        // Límites
        public int MaxPrinters { get; set; }
        public int MaxUsers { get; set; }
        public int MaxPolicies { get; set; }
        public long MaxStorageMB { get; set; }

        // Características
        public bool HasCostCalculation { get; set; }
        public bool HasAdvancedPolicies { get; set; }
        public bool HasScheduledReports { get; set; }
        public bool HasApiAccess { get; set; }
        public bool HasCustomReports { get; set; }
        public bool HasWhiteLabel { get; set; }
        public bool HasPrioritySupport { get; set; }

        public List<PlanFeatureDTO> Features { get; set; } = new();
    }

    /// <summary>
    /// DTO para características de plan
    /// </summary>
    public class PlanFeatureDTO
    {
        public Guid Id { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string FeatureDescription { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// DTO para suscripciones
    /// </summary>
    public class SubscriptionDTO
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public SubscriptionStatusEnum Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsTrial { get; set; }
        public int DaysRemaining { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public BillingCycle BillingCycle { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public List<PaymentDTO> RecentPayments { get; set; } = new();
    }

    /// <summary>
    /// DTO para pagos
    /// </summary>
    public class PaymentDTO
    {
        public Guid Id { get; set; } = default;
        public decimal Amount { get; set; } = 0;
        public string Currency { get; set; } = string.Empty;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        public DateTime PaymentDate { get; set; } = DateTime.MinValue;
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para crear suscripción
    /// </summary>
    public class CreateSubscriptionRequestDTO
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid PlanId { get; set; }

        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

        [StringLength(100)]
        public string StripeCustomerId { get; set; } = string.Empty;

        public bool StartTrial { get; set; } = false;
    }

    /// <summary>
    /// DTO para cambiar plan
    /// </summary>
    public class ChangePlanRequestDTO
    {
        [Required]
        public Guid SubscriptionId { get; set; }

        [Required]
        public Guid NewPlanId { get; set; }

        public BillingCycle NewBillingCycle { get; set; } = BillingCycle.Monthly;

        public bool Prorate { get; set; } = true;
    }

    /// <summary>
    /// DTO para cancelar suscripción
    /// </summary>
    public class CancelSubscriptionRequestDTO
    {
        [Required]
        public Guid SubscriptionId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool Immediate { get; set; } = false; // Si true, cancela inmediatamente, si false, al final del período
    }

    /// <summary>
    /// DTO para estadísticas de tenant
    /// </summary>
    public class TenantStatisticsDTO
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int TotalPrintJobs { get; set; }
        public int TotalPolicies { get; set; }
        public long StorageUsedMB { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.MinValue;
        public SubscriptionStatusEnum? SubscriptionStatus { get; set; } = SubscriptionStatusEnum.Unknown;
        public int DaysRemaining { get; set; } = 0;
    }

    /// <summary>
    /// DTO para dashboard del tenant
    /// </summary>
    public class TenantDashboardDTO
    {
        public TenantDTO Tenant { get; set; } = new();
        public CurrentSubscriptionDTO Subscription { get; set; } = new();
        public List<TenantStatisticsDTO> Statistics { get; set; } = new();
        public List<RecentActivityDTO> RecentActivities { get; set; } = new();
        public UsageSummaryDTO Usage { get; set; } = new();
    }

    /// <summary>
    /// DTO para actividad reciente
    /// </summary>
    public class RecentActivityDTO
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty; // UserLogin, PrintJob, PolicyViolation, etc.
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public string User { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resumen de uso
    /// </summary>
    public class UsageSummaryDTO
    {
        public int CurrentUsers { get; set; }
        public int MaxUsers { get; set; }
        public double UsersUsagePercentage { get; set; }

        public int CurrentPrinters { get; set; }
        public int MaxPrinters { get; set; }
        public double PrintersUsagePercentage { get; set; }

        public int CurrentPolicies { get; set; }
        public int MaxPolicies { get; set; }
        public double PoliciesUsagePercentage { get; set; }

        public long CurrentStorageMB { get; set; }
        public long MaxStorageMB { get; set; }
        public double StorageUsagePercentage { get; set; }

        public int PrintJobsToday { get; set; }
        public int PrintJobsThisMonth { get; set; }
        public decimal TotalCostThisMonth { get; set; }
    }

    /// <summary>
    /// DTO para configuración de tenant
    /// </summary>
    public class TenantSettingsDTO
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public int LowConsumableThreshold { get; set; }
        public int CriticalConsumableThreshold { get; set; }
    }

    /// <summary>
    /// DTO para verificar límites de plan
    /// </summary>
    public class PlanLimitsCheckDTO
    {
        public bool CanAddPrinter { get; set; }
        public bool CanAddUser { get; set; }
        public bool CanAddPolicy { get; set; }
        public bool CanUseStorage { get; set; }
        public bool HasFeatureEnabled { get; set; }

        public string Message { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}

