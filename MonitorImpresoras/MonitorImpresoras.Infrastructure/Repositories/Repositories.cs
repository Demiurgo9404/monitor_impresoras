using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;
using MonitorImpresoras.Domain.Enums;
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

        public async Task<IEnumerable<Printer>> GetPrintersByTenantIdAsync(Guid tenantId)
        {
            return await _context.Printers
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetOnlinePrintersAsync()
        {
            return await _context.Printers
                .Where(p => p.IsActive && p.IsOnline)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetOfflinePrintersAsync()
        {
            return await _context.Printers
                .Where(p => p.IsActive && !p.IsOnline)
                .ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetPrintersByLocationAsync(string location)
        {
            return await _context.Printers
                .Where(p => p.IsActive && p.Location == location)
                .ToListAsync();
        }

        public async Task<Printer> GetPrinterWithConsumablesAsync(Guid printerId)
        {
            return await _context.Printers
                .Include(p => p.Consumables)
                .FirstOrDefaultAsync(p => p.Id == printerId);
        }

        // Implementación de IRepository<Printer>
        public async Task<Printer?> GetByIdAsync(Guid id)
        {
            return await _context.Printers.FindAsync(id);
        }

        public async Task<IEnumerable<Printer>> GetAllAsync()
        {
            return await _context.Printers.ToListAsync();
        }

        public async Task<IEnumerable<Printer>> GetAllAsync(System.Linq.Expressions.Expression<Func<Printer, bool>> predicate)
        {
            return await _context.Printers.Where(predicate).ToListAsync();
        }

        public async Task<Printer?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Printer, bool>> predicate)
        {
            return await _context.Printers.FirstOrDefaultAsync(predicate);
        }

        public void Add(Printer entity)
        {
            _context.Printers.Add(entity);
        }

        public void AddRange(IEnumerable<Printer> entities)
        {
            _context.Printers.AddRange(entities);
        }

        public void Update(Printer entity)
        {
            _context.Printers.Update(entity);
        }

        public void Remove(Printer entity)
        {
            _context.Printers.Remove(entity);
        }

        public void RemoveRange(IEnumerable<Printer> entities)
        {
            _context.Printers.RemoveRange(entities);
        }

        public async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Printer, bool>> predicate)
        {
            return await _context.Printers.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Printer, bool>> predicate)
        {
            return await _context.Printers.CountAsync(predicate);
        }

        public async Task<IEnumerable<Printer>> GetAllAsync(System.Linq.Expressions.Expression<Func<Printer, bool>> predicate, System.Linq.Expressions.Expression<Func<Printer, object>> orderBy, int limit)
        {
            return await _context.Printers
                .Where(predicate)
                .OrderBy(orderBy)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Printer?> FindAsync(System.Linq.Expressions.Expression<Func<Printer, bool>> predicate)
        {
            return await _context.Printers.FirstOrDefaultAsync(predicate);
        }

        public async Task AddAsync(Printer entity)
        {
            await _context.Printers.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<Printer> entities)
        {
            await _context.Printers.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(Printer entity)
        {
            _context.Printers.Update(entity);
        }

        public async Task DeleteAsync(Printer entity)
        {
            _context.Printers.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<IEnumerable<User>> GetUsersByTenantIdAsync(Guid tenantId)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Where(u => u.IsActive && u.UserRoles.Any(ur => ur.Role.Name == roleName))
                .ToListAsync();
        }

        // Implementación de IRepository<User>
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.Where(predicate).ToListAsync();
        }

        public async Task<User?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.FirstOrDefaultAsync(predicate);
        }

        public void Add(User entity)
        {
            _context.Users.Add(entity);
        }

        public void AddRange(IEnumerable<User> entities)
        {
            _context.Users.AddRange(entities);
        }

        public void Update(User entity)
        {
            _context.Users.Update(entity);
        }

        public void Remove(User entity)
        {
            _context.Users.Remove(entity);
        }

        public void RemoveRange(IEnumerable<User> entities)
        {
            _context.Users.RemoveRange(entities);
        }

        public async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.CountAsync(predicate);
        }

        public async Task<IEnumerable<User>> GetAllAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, System.Linq.Expressions.Expression<Func<User, object>> orderBy, int limit)
        {
            return await _context.Users
                .Where(predicate)
                .OrderBy(orderBy)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<User?> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.FirstOrDefaultAsync(predicate);
        }

        public async Task AddAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<User> entities)
        {
            await _context.Users.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(User entity)
        {
            _context.Users.Update(entity);
        }

        public async Task DeleteAsync(User entity)
        {
            _context.Users.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }

    public class AlertRepository : IAlertRepository
    {
        private readonly ApplicationDbContext _context;

        public AlertRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Alert>> GetActiveAlertsAsync()
        {
            return await _context.Alerts
                .Where(a => a.Status == AlertStatus.New || a.Status == AlertStatus.InProgress)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetAlertsByTenantIdAsync(Guid tenantId)
        {
            return await _context.Alerts
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetAlertsBySeverityAsync(AlertSeverity severity)
        {
            return await _context.Alerts
                .Where(a => a.Severity == severity)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetAlertsByTypeAsync(AlertType alertType)
        {
            return await _context.Alerts
                .Where(a => a.AlertType == alertType)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Implementación de IRepository<Alert>
        public async Task<Alert?> GetByIdAsync(Guid id)
        {
            return await _context.Alerts.FindAsync(id);
        }

        public async Task<IEnumerable<Alert>> GetAllAsync()
        {
            return await _context.Alerts.ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetAllAsync(System.Linq.Expressions.Expression<Func<Alert, bool>> predicate)
        {
            return await _context.Alerts.Where(predicate).ToListAsync();
        }

        public async Task<Alert?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Alert, bool>> predicate)
        {
            return await _context.Alerts.FirstOrDefaultAsync(predicate);
        }

        public void Add(Alert entity)
        {
            _context.Alerts.Add(entity);
        }

        public void AddRange(IEnumerable<Alert> entities)
        {
            _context.Alerts.AddRange(entities);
        }

        public void Update(Alert entity)
        {
            _context.Alerts.Update(entity);
        }

        public void Remove(Alert entity)
        {
            _context.Alerts.Remove(entity);
        }

        public void RemoveRange(IEnumerable<Alert> entities)
        {
            _context.Alerts.RemoveRange(entities);
        }

        public async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<Alert, bool>> predicate)
        {
            return await _context.Alerts.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Alert, bool>> predicate)
        {
            return await _context.Alerts.CountAsync(predicate);
        }

        public async Task<IEnumerable<Alert>> GetAllAsync(System.Linq.Expressions.Expression<Func<Alert, bool>> predicate, System.Linq.Expressions.Expression<Func<Alert, object>> orderBy, int limit)
        {
            return await _context.Alerts
                .Where(predicate)
                .OrderBy(orderBy)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Alert?> FindAsync(System.Linq.Expressions.Expression<Func<Alert, bool>> predicate)
        {
            return await _context.Alerts.FirstOrDefaultAsync(predicate);
        }

        public async Task AddAsync(Alert entity)
        {
            await _context.Alerts.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<Alert> entities)
        {
            await _context.Alerts.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(Alert entity)
        {
            _context.Alerts.Update(entity);
        }

        public async Task DeleteAsync(Alert entity)
        {
            _context.Alerts.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Alert>> GetRecentAlertsByTypeAndEntityAsync(Guid entityId, string alertType, TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.UtcNow - timeSpan;
            return await _context.Alerts
                .Where(a => a.EntityId == entityId && a.Type.ToString() == alertType && a.CreatedAt >= cutoffTime)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetByConsumableIdAsync(Guid consumableId)
        {
            return await _context.Alerts
                .Where(a => a.EntityId == consumableId && a.Type.ToString().Contains("Consumable"))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }

    public class ConsumableRepository : IConsumableRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsumableRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PrinterConsumablePart>> GetConsumablesByPrinterIdAsync(Guid printerId)
        {
            return await _context.PrinterConsumableParts
                .Where(c => c.PrinterId == printerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrinterConsumablePart>> GetLowConsumablesAsync()
        {
            return await _context.PrinterConsumableParts
                .Where(c => c.CurrentLevel.HasValue &&
                           c.WarningLevel.HasValue &&
                           c.CurrentLevel.Value <= c.WarningLevel.Value)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrinterConsumablePart>> GetCriticalConsumablesAsync()
        {
            return await _context.PrinterConsumableParts
                .Where(c => c.CurrentLevel.HasValue &&
                           c.CriticalLevel.HasValue &&
                           c.CurrentLevel.Value <= c.CriticalLevel.Value)
                .ToListAsync();
        }

        // Implementación de IRepository<PrinterConsumablePart>
        public async Task<PrinterConsumablePart?> GetByIdAsync(Guid id)
        {
            return await _context.PrinterConsumableParts.FindAsync(id);
        }

        public async Task<IEnumerable<PrinterConsumablePart>> GetAllAsync()
        {
            return await _context.PrinterConsumableParts.ToListAsync();
        }

        public async Task<IEnumerable<PrinterConsumablePart>> GetAllAsync(System.Linq.Expressions.Expression<Func<PrinterConsumablePart, bool>> predicate)
        {
            return await _context.PrinterConsumableParts.Where(predicate).ToListAsync();
        }

        public async Task<PrinterConsumablePart?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<PrinterConsumablePart, bool>> predicate)
        {
            return await _context.PrinterConsumableParts.FirstOrDefaultAsync(predicate);
        }

        public void Add(PrinterConsumablePart entity)
        {
            _context.PrinterConsumableParts.Add(entity);
        }

        public void AddRange(IEnumerable<PrinterConsumablePart> entities)
        {
            _context.PrinterConsumableParts.AddRange(entities);
        }

        public void Update(PrinterConsumablePart entity)
        {
            _context.PrinterConsumableParts.Update(entity);
        }

        public void Remove(PrinterConsumablePart entity)
        {
            _context.PrinterConsumableParts.Remove(entity);
        }

        public void RemoveRange(IEnumerable<PrinterConsumablePart> entities)
        {
            _context.PrinterConsumableParts.RemoveRange(entities);
        }

        public async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<PrinterConsumablePart, bool>> predicate)
        {
            return await _context.PrinterConsumableParts.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<PrinterConsumablePart, bool>> predicate)
        {
            return await _context.PrinterConsumableParts.CountAsync(predicate);
        }

        public async Task<IEnumerable<PrinterConsumablePart>> GetAllAsync(System.Linq.Expressions.Expression<Func<PrinterConsumablePart, bool>> predicate, System.Linq.Expressions.Expression<Func<PrinterConsumablePart, object>> orderBy, int limit)
        {
            return await _context.PrinterConsumableParts
                .Where(predicate)
                .OrderBy(orderBy)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<PrinterConsumablePart?> FindAsync(System.Linq.Expressions.Expression<Func<PrinterConsumablePart, bool>> predicate)
        {
            return await _context.PrinterConsumableParts.FirstOrDefaultAsync(predicate);
        }

        public async Task AddAsync(PrinterConsumablePart entity)
        {
            await _context.PrinterConsumableParts.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<PrinterConsumablePart> entities)
        {
            await _context.PrinterConsumableParts.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(PrinterConsumablePart entity)
        {
            _context.PrinterConsumableParts.Update(entity);
        }

        public async Task DeleteAsync(PrinterConsumablePart entity)
        {
            _context.PrinterConsumableParts.Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
