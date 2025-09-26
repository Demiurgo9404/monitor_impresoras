using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Common;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        public virtual async Task AddAsync(T entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreatedAt = DateTime.UtcNow;
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
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

        public virtual void Update(T entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Remove(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
            }
            _dbSet.RemoveRange(entities);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            return predicate == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(predicate);
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
