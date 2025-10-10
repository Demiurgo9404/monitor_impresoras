using System.Linq.Expressions;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Interfaces
{
    // Interfaces para repositorios específicos
    public interface IPrintJobRepository : IRepository<PrintJob>
    {
        Task<IEnumerable<PrintJob>> GetJobsByPrinterIdAsync(Guid printerId);
        Task<IEnumerable<PrintJob>> GetJobsByUserIdAsync(string userId);
        Task<IEnumerable<PrintJob>> GetJobsByTenantIdAsync(Guid tenantId, DateTime startDate, DateTime endDate);
    }

    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    }

    public interface IReportRepository : IRepository<ReportTemplate>
    {
        Task<IEnumerable<ReportTemplate>> GetTemplatesByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<ReportTemplate>> GetTemplatesByTypeAsync(ReportType reportType);
        Task<IEnumerable<ReportTemplate>> GetActiveTemplatesAsync();
    }

    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<Subscription?> GetActiveSubscriptionByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<Subscription>> GetSubscriptionsByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync();
    }

    public interface IConsumableRepository : IRepository<PrinterConsumable>
    {
        Task<IEnumerable<PrinterConsumable>> GetConsumablesByPrinterIdAsync(Guid printerId);
        Task<IEnumerable<PrinterConsumable>> GetLowConsumablesAsync();
        Task<IEnumerable<PrinterConsumable>> GetCriticalConsumablesAsync();
    }
}

