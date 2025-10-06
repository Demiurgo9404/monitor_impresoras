using MonitorImpresoras.Domain.Entities;
using System.Security.Claims;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        Dictionary<string, string> GetClaimsFromToken(string token);
    }

    public interface ISubscriptionService
    {
        Task<Subscription?> GetActiveSubscriptionAsync(Guid userId);
        Task<Subscription> CreateSubscriptionAsync(Guid userId, SubscriptionPlan plan);
        Task<Invoice> CreateInvoiceAsync(Guid subscriptionId, decimal amount);
        Task MarkInvoiceAsPaidAsync(Guid invoiceId);
    }

    public interface IWindowsPrinterService
    {
        Task<List<Printer>> GetLocalPrintersAsync();
        Task<bool> IsPrinterOnlineAsync(string printerName);
    }

    public interface ISnmpService
    {
        Task<bool> TestConnectionAsync(string ipAddress, string community = "public");
        Task<Dictionary<string, object>> GetPrinterInfoAsync(string ipAddress, string community = "public");
    }

    public interface IPrinterRepository
    {
        Task<List<Printer>> GetAllAsync();
        Task<Printer?> GetByIdAsync(Guid id);
        Task<Printer> AddAsync(Printer printer);
        Task UpdateAsync(Printer printer);
        Task DeleteAsync(Guid id);
    }
}
