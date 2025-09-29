using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Repositories
{
    public class PrinterRepository : IPrinterRepository
    {
        private readonly ApplicationDbContext _context;

        public PrinterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Printer?> GetByIdAsync(Guid id)
        {
            return await _context.Printers.FindAsync(id);
        }

        public async Task<IEnumerable<Printer>> GetAllAsync()
        {
            return await _context.Printers.ToListAsync();
        }

        public async Task AddAsync(Printer printer)
        {
            await _context.Printers.AddAsync(printer);
        }

        public async Task UpdateAsync(Printer printer)
        {
            _context.Printers.Update(printer);
        }

        public async Task DeleteAsync(Guid id)
        {
            var printer = await GetByIdAsync(id);
            if (printer != null)
                _context.Printers.Remove(printer);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
