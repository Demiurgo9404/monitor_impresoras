using System.Linq.Expressions;
using MonitorImpresoras.Domain.Common;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Domain.Interfaces
{
    // Interfaces para repositorios específicos
    public interface IPrinterRepository : IRepository<Printer>
    {
        Task<IEnumerable<Printer>> GetPrintersByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<Printer>> GetOnlinePrintersAsync();
        Task<IEnumerable<Printer>> GetOfflinePrintersAsync();
        Task<IEnumerable<Printer>> GetPrintersNeedingMaintenanceAsync();
        Task<IEnumerable<Printer>> GetPrintersByLocationAsync(string location);
        Task<Printer> GetPrinterWithConsumablesAsync(Guid printerId);
        Task<IEnumerable<Printer>> GetPrintersWithErrorsAsync();
    }

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

    public interface IAlertRepository : IRepository<Alert>
    {
        Task<IEnumerable<Alert>> GetActiveAlertsAsync();
        Task<IEnumerable<Alert>> GetAlertsByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<Alert>> GetAlertsBySeverityAsync(AlertSeverity severity);
        Task<IEnumerable<Alert>> GetAlertsByTypeAsync(AlertType alertType);
        Task<IEnumerable<Alert>> GetRecentAlertsByTypeAndEntityAsync(Guid entityId, string alertType, TimeSpan timeSpan);
        Task<IEnumerable<Alert>> GetByConsumableIdAsync(Guid consumableId);
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

    public interface IConsumableRepository : IRepository<PrinterConsumablePart>
    {
        Task<IEnumerable<PrinterConsumablePart>> GetConsumablesByPrinterIdAsync(Guid printerId);
        Task<IEnumerable<PrinterConsumablePart>> GetLowConsumablesAsync();
        Task<IEnumerable<PrinterConsumablePart>> GetCriticalConsumablesAsync();
    }
}
