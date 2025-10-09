namespace QOPIQ.Web.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public UserInfo? User { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TenantInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CompanyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public int TotalProjects { get; set; }
        public int ActivePrinters { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProjectSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public int PrinterCount { get; set; }
        public int ActivePrinters { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardStats
    {
        public int TotalReports { get; set; }
        public int ScheduledReports { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalPrinters { get; set; }
        public int ActivePrinters { get; set; }
        public int ReportsThisMonth { get; set; }
        public List<ChartData> ReportsChart { get; set; } = new();
        public List<ChartData> PrintersChart { get; set; } = new();
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string? Color { get; set; }
    }
}

