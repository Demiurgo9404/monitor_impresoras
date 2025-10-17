using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces
{
    /// <summary>
    /// Interfaz base para repositorios genéricos
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Obtiene una entidad por ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad encontrada o null</returns>
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca entidades que cumplan con el predicado
        /// </summary>
        /// <param name="predicate">Predicado de búsqueda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades que cumplen el predicado</returns>
        Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega un rango de entidades
        /// </summary>
        /// <param name="entities">Entidades a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        void Update(TEntity entity);

        /// <summary>
        /// Actualiza un rango de entidades
        /// </summary>
        /// <param name="entities">Entidades a actualizar</param>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Elimina un rango de entidades
        /// </summary>
        /// <param name="entities">Entidades a eliminar</param>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla con el predicado
        /// </summary>
        /// <param name="predicate">Predicado de búsqueda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe al menos una entidad que cumpla el predicado</returns>
        Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta el número de entidades que cumplen con el predicado
        /// </summary>
        /// <param name="predicate">Predicado de búsqueda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades que cumplen el predicado</returns>
        Task<int> CountAsync(
            Expression<Func<TEntity, bool>> predicate = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda los cambios en el contexto
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

