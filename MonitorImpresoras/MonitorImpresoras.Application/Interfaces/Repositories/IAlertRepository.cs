using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces.Repositories
{
    public interface IAlertRepository : IRepository<Alert>
    {
        Task<IEnumerable<Alert>> GetActiveAlertsAsync();
        Task<IEnumerable<Alert>> GetAlertsByPrinterIdAsync(Guid printerId);
        Task<IEnumerable<Alert>> GetRecentAlertsByTypeAndEntityAsync(Guid entityId, string alertType, TimeSpan timeWindow);
        Task<IEnumerable<Alert>> GetByPrinterIdAsync(int printerId);
        Task<IEnumerable<Alert>> GetActiveAlertsByPrinterAsync(int printerId);
        Task<IEnumerable<Alert>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Alert> UpdateAsync(Alert alert);
        Task DeleteAsync(int id);
    }
}
