using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IWindowsPrinterService
    {
        Task<IEnumerable<string>> GetInstalledPrintersAsync();
        Task<bool> IsPrinterOnlineAsync(string printerName);
        Task<int> GetJobCountAsync(string printerName);
        Task<Dictionary<string, object>> GetPrinterStatusAsync(string printerName);
    }
}
