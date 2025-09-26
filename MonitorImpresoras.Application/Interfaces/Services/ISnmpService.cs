using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface ISnmpService
    {
        Task<Dictionary<string, string>> GetPrinterMetricsAsync(IPAddress ipAddress, string communityString = "public");
        Task<bool> IsPrinterOnlineAsync(IPAddress ipAddress, int timeoutMs = 2000);
    }
}
