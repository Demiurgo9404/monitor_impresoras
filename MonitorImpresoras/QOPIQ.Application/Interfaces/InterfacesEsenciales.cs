using QOPIQ.Domain.Entities;
using System.Security.Claims;

namespace QOPIQ.Application.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        Dictionary<string, string> GetClaimsFromToken(string token);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface ISubscriptionService
    {
        Task<Subscription?> GetActiveSubscriptionAsync(Guid userId);
        Task<Subscription> CreateSubscriptionAsync(Guid userId, SubscriptionPlan plan);
        Task<Invoice> CreateInvoiceAsync(Guid subscriptionId, decimal amount);
        Task MarkInvoiceAsPaidAsync(Guid invoiceId);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface IWindowsPrinterService
    {
        Task<List<Printer>> GetLocalPrintersAsync();
        Task<bool> IsPrinterOnlineAsync(string printerName);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface ISnmpService
    {
        Task<bool> TestConnectionAsync(string ipAddress, string community = "public");
        Task<Dictionary<string, object>> GetPrinterInfoAsync(string ipAddress, string community = "public");
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface IPrinterRepository
    {
        Task<List<Printer>> GetAllAsync();
        Task<Printer?> GetByIdAsync(Guid id);
        Task<Printer> AddAsync(Printer printer);
        Task UpdateAsync(Printer printer);
        Task DeleteAsync(Guid id);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface ITenantAccessor
    {
        string? TenantId { get; 
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}
        void SetTenant(string tenantId);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface ICompanyService
    {
        Task<List<Company>> GetAllAsync(string tenantId);
        Task<Company?> GetByIdAsync(Guid id, string tenantId);
        Task<Company> CreateAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Guid id, string tenantId);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface IProjectService
    {
        Task<List<Project>> GetAllAsync(string tenantId);
        Task<Project?> GetByIdAsync(Guid id, string tenantId);
        Task<Project> CreateAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Guid id, string tenantId);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface IReportService
    {
        Task<byte[]> GenerateReportAsync(string tenantId, string reportType, DateTime startDate, DateTime endDate);
        Task<List<Report>> GetReportsAsync(string tenantId);
        Task<Report?> GetReportByIdAsync(Guid id, string tenantId);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface IScheduledReportService
    {
        Task<List<ScheduledReport>> GetAllAsync(string tenantId);
        Task<ScheduledReport?> GetByIdAsync(Guid id, string tenantId);
        Task<ScheduledReport> CreateAsync(ScheduledReport scheduledReport);
        Task UpdateAsync(ScheduledReport scheduledReport);
        Task DeleteAsync(Guid id, string tenantId);
    
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}

    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ITenantService
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(Guid id);
    }}


