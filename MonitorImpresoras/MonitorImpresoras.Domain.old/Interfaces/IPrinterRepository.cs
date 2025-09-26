using MonitorImpresoras.Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MonitorImpresoras.Domain.Interfaces
{
    public interface IPrinterRepository : IRepository<Printer>
    {
        Task<IEnumerable<Printer>> GetAllPrintersWithConsumablesAsync();
        Task<Printer> GetPrinterWithConsumablesByIdAsync(int id);
        Task<IEnumerable<Printer>> GetPrintersByStatusAsync(string status);
        Task<IEnumerable<Printer>> GetPrintersByLocationAsync(string location);
        Task UpdatePrinterStatusAsync(int printerId, string status);
        Task UpdateConsumableLevelsAsync(int printerId, Dictionary<int, int> consumableLevels);
    }
}
