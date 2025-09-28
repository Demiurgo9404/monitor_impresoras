using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IAlertService
    {
        Task<Alert> CreateAlertAsync(CreateAlertDTO alertDto);
        Task<Alert?> GetAlertByIdAsync(Guid id);
        Task<IEnumerable<Alert>> GetAlertsByPrinterAsync(int printerId);
        Task<IEnumerable<Alert>> GetActiveAlertsAsync();
        Task<IEnumerable<Alert>> GetActiveAlertsByPrinterAsync(int printerId);
        Task<Alert> UpdateAlertAsync(Guid id, UpdateAlertDTO alertDto);
        Task<bool> DeleteAlertAsync(Guid id);
        Task<IEnumerable<Alert>> GetAlertsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<AlertStatisticsDTO> GetAlertStatisticsAsync();
    }
}
