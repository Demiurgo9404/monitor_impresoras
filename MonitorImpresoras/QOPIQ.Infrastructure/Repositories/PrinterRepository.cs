using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Infrastructure.Repositories
{
    public class PrinterRepository : IPrinterRepository
    {
        private readonly ApplicationDbContext _context;

        public PrinterRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Printer?> GetByIdAsync(Guid id)
        {
            return await _context.Printers.FindAsync(id);
        }

        public async Task<IEnumerable<Printer>> GetAllAsync()
        {
            return await _context.Printers.ToListAsync();
        }

        public async Task AddAsync(Printer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _context.Printers.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public void Update(Printer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Printers.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(Printer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Printers.Remove(entity);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<Printer> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _context.Printers.RemoveRange(entities);
            _context.SaveChanges();
        }

        // IPrinterRepository specific methods
        public async Task<Printer?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            return await _context.Printers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IpAddress == ipAddress, cancellationToken);
        }

        public async Task<IEnumerable<Printer>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken)
        {
            return await _context.Printers
                .Where(p => p.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Printer>> GetPrintersNeedingMaintenanceAsync(CancellationToken cancellationToken)
        {
            return await _context.Printers
                .Where(p => p.NeedsMaintenance)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Printer>> GetPrintersWithLowTonerAsync(int threshold = 20, CancellationToken cancellationToken = default)
        {
            return await _context.Printers
                .Where(p => p.TonerLevel <= threshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Printer>> SearchPrintersAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.Printers
                .Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Model.Contains(searchTerm) ||
                    p.IpAddress.Contains(searchTerm))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithIpAddressAsync(string ipAddress, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            return await _context.Printers
                .AnyAsync(p => p.IpAddress == ipAddress && (excludeId == null || p.Id != excludeId), cancellationToken);
        }

        public async Task<Printer> UpdateAsync(Printer entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
            entity.MarkAsUpdated();
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        // Explicit implementation of IRepository<Printer>.UpdateAsync
        async Task IRepository<Printer>.UpdateAsync(Printer entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
            entity.MarkAsUpdated();
            await _context.SaveChangesAsync();
        }

        // Explicit implementation of IRepository<Printer>.DeleteAsync
        async Task IRepository<Printer>.DeleteAsync(Printer entity)
        {
            await RemoveAsync(entity, default);
        }



        public async Task RemoveAsync(Printer entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Printers.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
