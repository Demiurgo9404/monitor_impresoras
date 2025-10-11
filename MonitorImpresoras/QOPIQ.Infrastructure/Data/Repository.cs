using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QOPIQ.Infrastructure.Data
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual void Update(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Set<TEntity>().Update(entity);
            _context.SaveChanges();
        }

        public virtual void Delete(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Set<TEntity>().Remove(entity);
            _context.SaveChanges();
        }
    }
}
