using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces.Repositories
{
    public interface IConsumableRepository : IRepository<PrinterConsumable>
    {
        Task<IEnumerable<PrinterConsumable>> GetConsumablesByPrinterIdAsync(int printerId);
        Task<PrinterConsumable?> GetCurrentLevelAsync(int printerId, string consumableType);
        Task<IEnumerable<PrinterConsumable>> GetLowConsumablesAsync();
    }
}
