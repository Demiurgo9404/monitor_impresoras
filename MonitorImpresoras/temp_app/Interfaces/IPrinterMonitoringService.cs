using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterMonitoringService
    {
        Task<PrinterStatus> GetPrinterStatusAsync(Printer printer);
        Task<Dictionary<string, string>> GetPrinterInfoViaSnmpAsync(Printer printer);
        Task<Dictionary<string, string>> GetPrinterInfoViaWmiAsync(string printerName);
        Task UpdatePrinterStatusAsync(Printer printer);
    }

    public class PrinterStatus
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; }
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
    }
}
