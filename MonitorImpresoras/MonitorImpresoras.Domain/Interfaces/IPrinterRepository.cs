using System.Linq.Expressions;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Domain.Interfaces
{
    public interface IPrinterRepository : IRepository<Printer>
    {
        Task<IEnumerable<Printer>> GetPrintersByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<Printer>> GetOnlinePrintersAsync();
        Task<IEnumerable<Printer>> GetOfflinePrintersAsync();
        Task<IEnumerable<Printer>> GetPrintersNeedingMaintenanceAsync();
        Task<IEnumerable<Printer>> GetPrintersByLocationAsync(string location);
        Task<Printer> GetPrinterWithConsumablesAsync(Guid printerId);
        Task<IEnumerable<Printer>> GetPrintersWithErrorsAsync();
    }
}
