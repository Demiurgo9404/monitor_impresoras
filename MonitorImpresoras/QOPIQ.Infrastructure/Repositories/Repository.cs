using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Interfaces;

namespace QOPIQ.Infrastructure.Repositories
{
    public class Repository<TEntity> : QOPIQ.Domain.Interfaces.IRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, object>> orderBy,
            int limit)
        {
            return await _dbSet
                .Where(predicate)
                .OrderBy(orderBy)
                .Take(limit)
                .ToListAsync();
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Add(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Add(entity);
            _context.SaveChanges();
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.AddRange(entities);
            _context.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void Remove(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        // Additional helper methods
        public virtual IQueryable<TEntity> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.RemoveRange(entities);
            _context.SaveChanges();
        }
    }
}
