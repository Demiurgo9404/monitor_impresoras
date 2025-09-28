using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterStatusService
    {
        Task<IEnumerable<PrinterStatus>> GetAllPrintersStatusAsync();
    }
}
