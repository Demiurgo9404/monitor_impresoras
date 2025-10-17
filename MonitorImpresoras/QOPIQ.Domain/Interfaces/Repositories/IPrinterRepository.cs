using QOPIQ.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces.Repositories
{
    public interface IPrinterRepository : IRepository<Printer>
    {
        Task<Printer?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);
        Task<IEnumerable<Printer>> GetPrintersNeedingMaintenanceAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Printer>> GetPrintersWithLowTonerAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Printer>> SearchPrintersAsync(string term, CancellationToken cancellationToken = default);
    }
}
