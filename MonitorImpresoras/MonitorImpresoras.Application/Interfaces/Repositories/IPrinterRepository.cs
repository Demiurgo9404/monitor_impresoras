using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces.Repositories
{
    public interface IPrinterRepository : IRepository<Printer>
    {
        Task<Printer> GetByIpAddressAsync(string ipAddress);
        Task<IEnumerable<Printer>> GetPrintersByDepartmentAsync(Guid departmentId);
    }
}
