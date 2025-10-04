using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterMonitoringService
    {
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Printer?> GetPrinterByIdAsync(Guid id);
        Task<Printer> AddPrinterAsync(Printer printer);
        Task UpdatePrinterAsync(Printer printer);
        Task DeletePrinterAsync(Guid id);
        Task CheckPrinterStatusAsync(Guid printerId);
        Task CheckAllPrintersStatusAsync();
    }
}
