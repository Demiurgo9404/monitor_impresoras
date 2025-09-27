using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface ISnmpService
    {
        Task<PrinterStatusInfo> GetPrinterStatusAsync(string ipAddress, string community = "public", int port = 161);
    }
}
