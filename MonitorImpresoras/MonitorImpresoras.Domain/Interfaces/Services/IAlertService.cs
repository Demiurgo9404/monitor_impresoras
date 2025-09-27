using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Domain.Interfaces.Services
{
    public interface IAlertService
    {
        Task<AlertDTO> GetAlertByIdAsync(Guid id);
        Task<IEnumerable<AlertDTO>> GetAlertsByFilterAsync(AlertFilterDTO filter);
        Task<AlertDTO> CreateAlertAsync(CreateAlertDTO alertDto);
        Task<AlertDTO> UpdateAlertAsync(Guid id, UpdateAlertDTO alertDto, string userId);
        Task<AlertStatsDTO> GetAlertStatsAsync();
        Task AcknowledgeAlertAsync(Guid id, string userId);
        Task ResolveAlertAsync(Guid id, string resolutionNotes, string userId);
    }
}
