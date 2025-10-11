using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Interfaces.Services
{
    public interface IPrinterMonitoringService
    {
        Task<IEnumerable<Printer>> GetAllPrintersAsync();
        Task<Printer?> GetPrinterByIdAsync(Guid id);
        Task AddPrinterAsync(Printer printer);
        Task UpdatePrinterAsync(Printer printer);
        Task DeletePrinterAsync(Guid id);
        Task<bool> TestPrinterConnectionAsync(string ipAddress);
        Task<PrinterStatus> GetPrinterStatusAsync(Guid id);
        Task MonitorPrintersAsync(CancellationToken cancellationToken = default);
    }
}
