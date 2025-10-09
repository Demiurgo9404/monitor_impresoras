using Microsoft.EntityFrameworkCore;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Data;

namespace QOPIQ.Infrastructure.Repositories
{
    public class PrinterRepository : IPrinterRepository
    {
        private readonly ApplicationDbContext _context;

        public PrinterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Printer>> GetAllAsync()
        {
            return await _context.Printers.ToListAsync();
        }

        public async Task<Printer?> GetByIdAsync(Guid id)
        {
            return await _context.Printers.FindAsync(id);
        }

        public async Task<Printer> AddAsync(Printer printer)
        {
            _context.Printers.Add(printer);
            await _context.SaveChangesAsync();
            return printer;
        }

        public async Task UpdateAsync(Printer printer)
        {
            _context.Printers.Update(printer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var printer = await GetByIdAsync(id);
            if (printer != null)
            {
                _context.Printers.Remove(printer);
                await _context.SaveChangesAsync();
            }
        }
    }
}

