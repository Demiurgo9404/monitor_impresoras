using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface IPrinterMonitoringService
    {
        Task<PrinterStatus> GetPrinterStatusAsync(Printer printer);
        Task UpdateAllPrintersStatusAsync();
    }

    public class PrinterStatus
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; }
        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();
        public int? PageCount { get; set; }
    }
}
