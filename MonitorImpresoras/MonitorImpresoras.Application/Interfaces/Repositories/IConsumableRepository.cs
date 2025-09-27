using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces.Repositories
{
    public interface IConsumableRepository : IRepository<PrinterConsumable>
    {
        Task<IEnumerable<PrinterConsumable>> GetConsumablesByPrinterIdAsync(Guid printerId);
        Task<PrinterConsumable?> GetCurrentLevelAsync(Guid printerId, string consumableType);
        Task<IEnumerable<PrinterConsumable>> GetLowConsumablesAsync();
    }
}
