using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface IPrinterMonitoringService
    {
        Task<IEnumerable<PrinterDto>> GetAllPrintersAsync();
        Task<PrinterDto> GetPrinterByIdAsync(Guid id);
        Task<PrinterDto> AddPrinterAsync(PrinterDto printerDto);
        Task UpdatePrinterAsync(PrinterDto printerDto);
        Task DeletePrinterAsync(Guid id);
        Task CheckPrinterStatusAsync(Guid printerId);
        Task CheckAllPrintersStatusAsync();
    }
}
