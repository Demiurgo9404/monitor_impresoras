using MonitorImpresoras.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterMonitoringService
    {
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Printer> GetPrinterByIdAsync(int id);
        Task<Printer> AddPrinterAsync(Printer printer);
        Task UpdatePrinterAsync(Printer printer);
        Task DeletePrinterAsync(int id);
        Task CheckPrinterStatusAsync(int printerId);
    }
}
