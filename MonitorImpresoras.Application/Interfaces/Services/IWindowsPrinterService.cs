using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface IWindowsPrinterService
    {
        Task<int?> GetPrinterPageCountAsync(string printerName);
        Task<IEnumerable<string>> GetLocalPrintersAsync();
    }
}
