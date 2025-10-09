using System.Linq.Expressions;

namespace QOPIQ.Domain.Interfaces
{
    /// <summary>
    /// Interfaz base para repositorios genéricos
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad</typeparam>
    public interface IRepository<TEntity>
    {
        /// <summary>
        /// Obtiene una entidad por ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <returns>Entidad encontrada o null</returns>
        Task<TEntity?> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <returns>Lista de entidades</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Obtiene entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>Entidades filtradas</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Obtiene entidades ordenadas con límite
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <param name="orderBy">Ordenamiento</param>
        /// <param name="limit">Límite de resultados</param>
        /// <returns>Entidades filtradas y ordenadas</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, int limit);

        /// <summary>
        /// Obtiene la primera entidad que cumple una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>Primera entidad o null</returns>
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Agrega múltiples entidades
        /// </summary>
        /// <param name="entities">Entidades a agregar</param>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Actualiza una entidad
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>True si existe</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Cuenta las entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Condición</param>
        /// <returns>Número de entidades</returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Busca una entidad usando un predicado
        /// </summary>
        /// <param name="predicate">Condición de búsqueda</param>
        /// <returns>Entidad encontrada o null</returns>
        Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Métodos sincrónicos para compatibilidad
        /// </summary>
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}

