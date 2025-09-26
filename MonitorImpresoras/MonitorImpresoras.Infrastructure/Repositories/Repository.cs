using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Interfaces;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación genérica del repositorio usando Entity Framework Core
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor del repositorio
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Obtiene una entidad por ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <returns>Entidad encontrada o null</returns>
        public async Task<T?> GetByIdAsync(Guid id) =>
            await _dbSet.FindAsync(id);

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <returns>Lista de entidades</returns>
        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _dbSet.ToListAsync();

        /// <summary>
        /// Obtiene entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>Entidades filtradas</returns>
        public async Task<IEnumerable<T>> GetAllAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        /// <summary>
        /// Obtiene entidades ordenadas con límite
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <param name="orderBy">Ordenamiento</param>
        /// <param name="limit">Límite de resultados</param>
        /// <returns>Entidades filtradas y ordenadas</returns>
        public async Task<IEnumerable<T>> GetAllAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, System.Linq.Expressions.Expression<Func<T, object>> orderBy, int limit)
        {
            var query = _dbSet.Where(predicate).OrderBy(orderBy);
            return await query.Take(limit).ToListAsync();
        }

        /// <summary>
        /// Obtiene la primera entidad que cumple una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>Primera entidad o null</returns>
        public async Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
            await _dbSet.FirstOrDefaultAsync(predicate);

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Agrega múltiples entidades
        /// </summary>
        /// <param name="entities">Entidades a agregar</param>
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza una entidad
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>True si existe</returns>
        public async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        /// <summary>
        /// Cuenta las entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>Número de entidades</returns>
        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
            await _dbSet.CountAsync(predicate);

        /// <summary>
        /// Métodos sincrónicos para compatibilidad
        /// </summary>
        public void Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            _context.SaveChanges();
        }
    }
}
