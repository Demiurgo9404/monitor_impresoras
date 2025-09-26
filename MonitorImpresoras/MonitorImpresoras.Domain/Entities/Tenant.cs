using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para representar un tenant en el sistema multi-tenant
    /// </summary>
    public class Tenant : BaseEntity
    {
        public string TenantKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string Timezone { get; set; } = "America/Mexico_City";
        public string Currency { get; set; } = "MXN";
        public string DatabaseName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;

        // Límites del plan
        public int MaxPrinters { get; set; } = 5;
        public int MaxUsers { get; set; } = 10;
        public int MaxPolicies { get; set; } = 5;
        public long MaxStorageMB { get; set; } = 100;

        // Configuración
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool SmsNotificationsEnabled { get; set; } = false;
        public int LowConsumableThreshold { get; set; } = 20;
        public int CriticalConsumableThreshold { get; set; } = 10;

        // Estadísticas
        public DateTime? LastActivity { get; set; }
        public int TotalPrintJobs { get; set; } = 0;
        public decimal TotalCost { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<TenantUser> Users { get; set; } = new List<TenantUser>();
        public virtual ICollection<Printer> Printers { get; set; } = new List<Printer>();
        public virtual ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
        public virtual ICollection<PrintingPolicy> Policies { get; set; } = new List<PrintingPolicy>();
        public virtual ICollection<CostPolicy> CostPolicies { get; set; } = new List<CostPolicy>();
        public virtual ICollection<ScheduledReport> ScheduledReports { get; set; } = new List<ScheduledReport>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
