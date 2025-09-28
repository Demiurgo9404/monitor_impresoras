using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterStatusService
    {
        Task<IEnumerable<PrinterStatusInfo>> GetAllPrintersStatusAsync();
    }
}
