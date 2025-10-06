using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterMonitoringService
    {
        Task<List<Printer>> GetAllPrintersAsync();
        Task<Printer?> GetPrinterByIdAsync(Guid id);
        Task<Printer> AddPrinterAsync(Printer printer);
        Task UpdatePrinterAsync(Printer printer);
        Task DeletePrinterAsync(Guid id);
        Task<bool> TestPrinterConnectionAsync(string ipAddress);
        Task<Dictionary<string, object>> GetPrinterStatusAsync(Guid printerId);
    }
}
