using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces.Repositories
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
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca entidades que cumplan con el predicado
        /// </summary>
        /// <param name="predicate">Predicado de búsqueda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades que cumplen el predicado</returns>
        Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad agregada</returns>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        void Update(TEntity entity);

        /// <summary>
        /// Actualiza una entidad existente de forma asíncrona
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un rango de entidades
        /// </summary>
        /// <param name="entities">Entidades a actualizar</param>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Actualiza un rango de entidades de forma asíncrona
        /// </summary>
        /// <param name="entities">Entidades a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Elimina una entidad de forma asíncrona
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un rango de entidades
        /// </summary>
        /// <param name="entities">Entidades a eliminar</param>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Elimina un rango de entidades de forma asíncrona
        /// </summary>
        /// <param name="entities">Entidades a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda los cambios en el contexto
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
