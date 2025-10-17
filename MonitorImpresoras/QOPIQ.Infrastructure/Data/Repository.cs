using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Interfaces.Repositories;

namespace QOPIQ.Infrastructure.Data
{
    /// <summary>
    /// Implementación base del patrón Repository para el acceso a datos
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            if (predicate == null) 
                throw new ArgumentNullException(nameof(predicate));
                
            return await _dbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
                
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) 
                throw new ArgumentNullException(nameof(entities));
                
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual void Update(TEntity entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
                
            _dbSet.Update(entity);
        }

        public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Update(entity);
            return Task.CompletedTask;
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            if (entities == null) 
                throw new ArgumentNullException(nameof(entities));
                
            _dbSet.UpdateRange(entities);
        }

        public virtual Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            UpdateRange(entities);
            return Task.CompletedTask;
        }

        public virtual void Remove(TEntity entity)
        {
            if (entity == null) 
                throw new ArgumentNullException(nameof(entity));
                
            _dbSet.Remove(entity);
        }

        public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Remove(entity);
            return Task.CompletedTask;
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities == null) 
                throw new ArgumentNullException(nameof(entities));
                
            _dbSet.RemoveRange(entities);
        }

        public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            if (predicate == null) 
                throw new ArgumentNullException(nameof(predicate));
                
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            if (predicate == null) 
                throw new ArgumentNullException(nameof(predicate));
                
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
