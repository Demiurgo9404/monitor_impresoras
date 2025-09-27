using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces.Repositories
{
    public interface IConsumableRepository : IRepository<PrinterConsumable>
    {
        Task<IEnumerable<PrinterConsumable>> GetConsumablesByPrinterIdAsync(Guid printerId);
    }
}
