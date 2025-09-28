using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface ISnmpService
    {
        Task<Dictionary<string, string>> GetPrinterInfoAsync(string ipAddress, string community = "public");
        Task<int> GetPrinterPageCountAsync(string ipAddress, string community = "public");
        Task<int> GetTonerLevelAsync(string ipAddress, string color, string community = "public");
        Task<bool> IsPrinterOnlineAsync(string ipAddress, int timeout = 1000);
    }
}
