using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Infrastructure.Services.WMI
{
    public interface IWindowsPrinterService
    {
        Task<Dictionary<string, object>> GetPrinterStatusAsync(string printerName);
        Task<IEnumerable<PrinterDto>> GetLocalPrintersAsync();
        Task<bool> IsPrinterOnlineAsync(string printerName);
        Task<int> GetPrinterPageCountAsync(string printerName);
        Task<Dictionary<string, object>> CheckPrinterStatusAsync(string printerName);
    }
}
