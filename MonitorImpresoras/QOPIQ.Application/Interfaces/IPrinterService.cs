using QOPIQ.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QOPIQ.Application.Interfaces
{
    public interface IPrinterService
    {
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Printer?> GetPrinterByIdAsync(Guid id);
        Task<Printer> CreatePrinterAsync(Printer printer);
        Task<bool> UpdatePrinterAsync(Printer printer);
        Task<bool> DeletePrinterAsync(Guid id);
        Task<bool> CheckPrinterStatusAsync(string ipAddress);
    }
}
