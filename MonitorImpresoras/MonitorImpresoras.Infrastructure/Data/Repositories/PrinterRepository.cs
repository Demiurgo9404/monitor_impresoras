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
    public class PrinterRepository : IPrinterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Printer> _dbSet;

        public PrinterRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<Printer>();
        }

        public async Task<Printer> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Printer>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Printer>> FindAsync(Expression<Func<Printer, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Printer> SingleOrDefaultAsync(Expression<Func<Printer, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        public async Task AddAsync(Printer entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreatedAt = DateTime.UtcNow;
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<Printer> entities)
        {
            var baseEntities = entities.OfType<BaseEntity>();
            var now = DateTime.UtcNow;
            foreach (var entity in baseEntities)
            {
                entity.CreatedAt = now;
                entity.UpdatedAt = now;
            }
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(Printer entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(Printer entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<Printer> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<bool> AnyAsync(Expression<Func<Printer, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<Printer, bool>> predicate = null)
        {
            return predicate == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(predicate);
        }

        public async Task<IEnumerable<Printer>> GetAllPrintersWithConsumablesAsync()
        {
            return await _dbSet
                .Include(p => p.Consumables)
                .ToListAsync();
        }

        public async Task<Printer> GetPrinterWithConsumablesByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Consumables)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Printer>> GetPrintersByStatusAsync(string status)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetPrintersByLocationAsync(string location)
        {
            return await _dbSet
                .Where(p => p.Location != null && p.Location.Contains(location))
                .ToListAsync();
        }

        public async Task UpdatePrinterStatusAsync(int printerId, string status)
        {
            var printer = await GetByIdAsync(printerId);
            if (printer != null)
            {
                printer.Status = status;
                Update(printer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateConsumableLevelsAsync(int printerId, Dictionary<int, int> consumableLevels)
        {
            var printer = await _context.Printers
                .Include(p => p.Consumables)
                .FirstOrDefaultAsync(p => p.Id == printerId);

            if (printer != null)
            {
                foreach (var (consumableId, level) in consumableLevels)
                {
                    var consumable = printer.Consumables.FirstOrDefault(c => c.Id == consumableId);
                    if (consumable != null)
                    {
                        consumable.CurrentLevel = level;
                        consumable.LastUpdated = System.DateTime.UtcNow;
                    }
                }
                
                await _context.SaveChangesAsync();
            }
        }
    }
}
