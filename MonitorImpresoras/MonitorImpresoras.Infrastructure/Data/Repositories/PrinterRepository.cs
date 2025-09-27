using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.Infrastructure.Data.Repositories
{
    public class PrinterRepository : Repository<Printer>, IPrinterRepository
    {
        public PrinterRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Implementación de métodos específicos de IPrinterRepository
        public async Task<IEnumerable<Printer>> GetPrintersByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetOnlinePrintersAsync()
        {
            return await _dbSet
                .Where(p => p.IsOnline && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetOfflinePrintersAsync()
        {
            return await _dbSet
                .Where(p => !p.IsOnline && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetPrintersNeedingMaintenanceAsync()
        {
            return await _dbSet
                .Where(p => p.NeedsMaintenance && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetPrintersByLocationAsync(string location)
        {
            return await _dbSet
                .Where(p => p.Location != null && p.Location.Contains(location))
                .ToListAsync();
        }

        public async Task<Printer> GetPrinterWithConsumablesAsync(Guid printerId)
        {
            return await _dbSet
                .Include(p => p.ConsumableParts)
                .FirstOrDefaultAsync(p => p.Id == printerId);
        }

        public async Task<IEnumerable<Printer>> GetPrintersWithErrorsAsync()
        {
            return await _dbSet
                .Where(p => p.HasError && p.IsActive)
                .ToListAsync();
        }
    }
}
