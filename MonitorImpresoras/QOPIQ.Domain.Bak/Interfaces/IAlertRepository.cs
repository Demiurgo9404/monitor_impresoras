using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Interfaces
{
    public interface IAlertRepository : IRepository<Alert>
    {
        Task<IEnumerable<Alert>> GetActiveAlertsAsync();
        Task<IEnumerable<Alert>> GetAlertsByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<Alert>> GetAlertsBySeverityAsync(AlertSeverity severity);
        Task<IEnumerable<Alert>> GetAlertsByTypeAsync(AlertType alertType);
        Task<IEnumerable<Alert>> GetRecentAlertsByTypeAndEntityAsync(Guid entityId, string alertType, TimeSpan timeSpan);
        Task<IEnumerable<Alert>> GetByConsumableIdAsync(Guid consumableId);
    }
}

