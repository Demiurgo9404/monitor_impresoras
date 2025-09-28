using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterService
    {
        Task<Printer?> GetPrinterByIdAsync(Guid id);
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Printer> CreatePrinterAsync(Printer printer);
        Task<Printer?> UpdatePrinterAsync(Guid id, Printer updatedPrinter);
        Task<bool> DeletePrinterAsync(Guid id);
    }
}
