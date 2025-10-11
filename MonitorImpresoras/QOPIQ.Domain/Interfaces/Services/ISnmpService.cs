using System.Collections.Generic;
using System.Threading.Tasks;
using QOPIQ.Domain.DTOs;

namespace QOPIQ.Domain.Interfaces.Services
{
    public interface ISnmpService
    {
        Task<Dictionary<string, object>> GetPrinterInfoAsync(string ipAddress, string community = "public");
        Task<string> GetPrinterStatusAsync(string ipAddress, string community = "public");
        Task<PrinterCountersDto> GetPrinterCountersAsync(string ip, string community = "public");
    }
}
