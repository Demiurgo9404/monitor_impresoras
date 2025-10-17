using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Interfaces;

namespace QOPIQ.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual void Update(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
            
            // Set UpdatedAt if the entity has the property
            var updatedAtProperty = entity.GetType().GetProperty("UpdatedAt");
            if (updatedAtProperty != null && updatedAtProperty.CanWrite)
            {
                updatedAtProperty.SetValue(entity, DateTime.UtcNow, null);
            }
            
            _dbSet.Update(entity);
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
                
                // Set UpdatedAt if the entity has the property
                var updatedAtProperty = entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.CanWrite)
                {
                    updatedAtProperty.SetValue(entity, DateTime.UtcNow, null);
                }
            }
            
            _dbSet.UpdateRange(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.RemoveRange(entities);
        }

        public virtual async Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<TEntity, bool>> predicate = null, 
            CancellationToken cancellationToken = default)
        {
            return predicate != null 
                ? await _dbSet.CountAsync(predicate, cancellationToken) 
                : await _dbSet.CountAsync(cancellationToken);
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual IQueryable<TEntity> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}
