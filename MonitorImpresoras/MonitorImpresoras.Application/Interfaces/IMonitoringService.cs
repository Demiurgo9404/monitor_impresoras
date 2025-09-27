using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IMonitoringService
    {
        Task<PrinterStatusInfo> GetPrinterStatusAsync(Printer printer);
        Task<IEnumerable<PrinterStatusInfo>> GetAllPrintersStatusAsync();
        Task<bool> IsPrinterOnlineAsync(string ipAddress);
        Task<PrinterStatusInfo> GetPrinterInfoAsync(string ipAddress);
        Task<IEnumerable<PrinterConsumable>> GetPrinterConsumablesAsync(string ipAddress);
        Task<bool> TestPrinterConnectionAsync(string ipAddress);
    }
}
