using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Common
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Guarda todos los cambios realizados en el contexto de la base de datos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El número de entradas de estado escritas en la base de datos.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inicia una nueva transacción de base de datos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma la transacción actual.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Revierte la transacción actual.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un repositorio para la entidad especificada.
        /// </summary>
        /// <typeparam name="TEntity">El tipo de entidad.</typeparam>
        /// <returns>Una instancia del repositorio.</returns>
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IEntity;
    }
}
