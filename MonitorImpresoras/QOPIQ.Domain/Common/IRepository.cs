using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Common
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        /// <summary>
        /// Obtiene todas las entidades.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Una lista de todas las entidades.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una entidad por su identificador.
        /// </summary>
        /// <param name="id">El identificador de la entidad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>La entidad si se encuentra; de lo contrario, null.</returns>
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca entidades que cumplan con la condición especificada.
        /// </summary>
        /// <param name="predicate">La condición para filtrar las entidades.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Una lista de entidades que cumplen con la condición.</returns>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva entidad al repositorio.
        /// </summary>
        /// <param name="entity">La entidad a agregar.</param>
        void Add(TEntity entity);

        /// <summary>
        /// Agrega un rango de entidades al repositorio.
        /// </summary>
        /// <param name="entities">Las entidades a agregar.</param>
        void AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Actualiza una entidad existente.
        /// </summary>
        /// <param name="entity">La entidad a actualizar.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Elimina una entidad del repositorio.
        /// </summary>
        /// <param name="entity">La entidad a eliminar.</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Elimina un rango de entidades del repositorio.
        /// </summary>
        /// <param name="entities">Las entidades a eliminar.</param>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Obtiene una consulta que permite realizar operaciones adicionales.
        /// </summary>
        /// <returns>Una consulta de entidades.</returns>
        IQueryable<TEntity> AsQueryable();

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla con la condición especificada.
        /// </summary>
        /// <param name="predicate">La condición para verificar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Verdadero si existe al menos una entidad que cumple la condición; de lo contrario, falso.</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el número total de entidades que cumplen con la condición especificada.
        /// </summary>
        /// <param name="predicate">La condición para contar las entidades.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El número de entidades que cumplen con la condición.</returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    }
}
