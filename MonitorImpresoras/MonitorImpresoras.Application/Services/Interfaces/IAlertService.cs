using MonitorImpresoras.Domain.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Services.Interfaces
{
    public interface IAlertService
    {
        Task SendAlertAsync(string message, string severity);
        Task<Alert> CreateAlertAsync(CreateAlertDTO dto);
    }
}
