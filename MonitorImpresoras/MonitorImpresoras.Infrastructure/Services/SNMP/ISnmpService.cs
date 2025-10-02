using System.Net;
using System.Threading.Tasks;

namespace MonitorImpresoras.Infrastructure.Services.SNMP
{
    public interface ISnmpService
    {
        Task<string> GetPrinterStatusAsync(string ipAddress, string communityString = "public", int port = 161);
        Task<int> GetInkLevelAsync(string ipAddress, string oid, string communityString = "public", int port = 161);
        Task<int> GetTonerLevelAsync(string ipAddress, string oid, string communityString = "public", int port = 161);
        Task<int> GetPageCountAsync(string ipAddress, string communityString = "public", int port = 161);
    }
}
