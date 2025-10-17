using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Infrastructure.Data.Repositories
{
    public class PrinterRepository : Repository<Printer>, IPrinterRepository
    {
        private new readonly AppDbContext _context;

        public PrinterRepository(AppDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Printer?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            return await _context.Printers
                .FirstOrDefaultAsync(p => p.IpAddress == ipAddress, cancellationToken);
        }

        public async Task<IEnumerable<Printer>> GetPrintersNeedingMaintenanceAsync(CancellationToken cancellationToken = default)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return await _context.Printers
                .Where(p => p.LastMaintenanceDate == null || 
                           p.LastMaintenanceDate < thirtyDaysAgo)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithIpAddressAsync(string ipAddress, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            return excludeId.HasValue
                ? await _context.Printers.AnyAsync(p => 
                    p.IpAddress == ipAddress && p.Id != excludeId, cancellationToken)
                : await _context.Printers.AnyAsync(p => 
                    p.IpAddress == ipAddress, cancellationToken);
        }

        public async Task<IEnumerable<Printer>> GetPrintersByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Printers
                .Where(p => p.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Printer>> GetPrintersWithLowTonerAsync(CancellationToken cancellationToken = default)
        {
            // Consider anything below 20% as low toner
            return await _context.Printers
                .Where(p => p.TonerLevelPercentage < 20)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Printer>> SearchPrintersAsync(string term, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await _context.Printers.ToListAsync(cancellationToken);

            term = term.ToLower();
            return await _context.Printers
                .Where(p => p.Name.ToLower().Contains(term) || 
                           p.Model.ToLower().Contains(term) || 
                           p.IpAddress.Contains(term) ||
                           p.SerialNumber.ToLower().Contains(term))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsIpAddressUniqueAsync(string ipAddress, Guid? excludePrinterId = null, CancellationToken cancellationToken = default)
        {
            return !await _context.Printers
                .AnyAsync(p => p.IpAddress == ipAddress && 
                             (!excludePrinterId.HasValue || p.Id != excludePrinterId.Value), 
                    cancellationToken);
        }

        public async Task<int> GetPrinterCountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Printers
                .CountAsync(p => p.TenantId == tenantId, cancellationToken);
        }

        public new async Task<Printer> UpdateAsync(Printer entity, CancellationToken cancellationToken = default)
        {
            _context.Printers.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public new async Task RemoveAsync(Printer entity, CancellationToken cancellationToken = default)
        {
            _context.Printers.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
