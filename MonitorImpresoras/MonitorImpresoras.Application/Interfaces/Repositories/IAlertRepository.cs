using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces.Repositories
{
    public interface IAlertRepository : IRepository<Alert>
    {
        Task<IEnumerable<Alert>> GetActiveAlertsAsync();
        Task<IEnumerable<Alert>> GetAlertsByPrinterIdAsync(Guid printerId);
        Task<IEnumerable<Alert>> GetRecentAlertsByTypeAndEntityAsync(Guid entityId, string alertType, TimeSpan timeWindow);
    }
}
