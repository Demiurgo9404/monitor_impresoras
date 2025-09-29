using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterRepository
    {
        Task<Printer?> GetByIdAsync(Guid id);
        Task<IEnumerable<Printer>> GetAllAsync();
        Task AddAsync(Printer printer);
        Task UpdateAsync(Printer printer);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
