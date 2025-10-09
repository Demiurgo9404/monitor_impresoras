using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.DTOs
{
    // DTOs para tenants
    public class CreateTenantDto
    {
        public string TenantKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public SubscriptionTier SubscriptionTier { get; set; }
    }

    public class TenantDto
    {
        public int Id { get; set; }
        public string TenantKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public SubscriptionTier SubscriptionTier { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // DTOs para reportes programados
    public class ReportRequestDto
    {
        public int TenantId { get; set; }
        public ReportType ReportType { get; set; }
        public ReportFormat Format { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    // DTOs para autenticación
}

