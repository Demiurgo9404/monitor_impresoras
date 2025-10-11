using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.Interfaces.Services
{
    public interface IPrinterService
    {
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Printer?> GetPrinterByIdAsync(Guid id);
        Task<Printer> CreatePrinterAsync(Printer printer);
        Task<bool> UpdatePrinterAsync(Printer printer);
        Task<bool> DeletePrinterAsync(Guid id);
    }
}
