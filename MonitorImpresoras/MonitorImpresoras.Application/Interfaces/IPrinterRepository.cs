using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterRepository
    {
        Task<IEnumerable<Printer>> GetAllAsync();
        Task<Printer?> GetByIdAsync(Guid id);
        Task<Printer> AddAsync(Printer printer);
        Task UpdateAsync(Printer printer);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}
