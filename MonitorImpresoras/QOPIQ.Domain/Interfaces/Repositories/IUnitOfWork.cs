using System;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Define la interfaz para el patrón Unit of Work, que coordina
    /// el trabajo de múltiples repositorios creando una única transacción de base de datos.
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Guarda todos los cambios realizados en el contexto de la base de datos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>El número de entidades escritas en la base de datos.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inicia una nueva transacción de base de datos de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma la transacción actual de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Revierte la transacción actual de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el repositorio para la entidad especificada.
        /// </summary>
        /// <typeparam name="TEntity">Tipo de entidad.</typeparam>
        /// <typeparam name="TRepository">Tipo de repositorio.</typeparam>
        /// <returns>Instancia del repositorio solicitado.</returns>
        TRepository GetRepository<TEntity, TRepository>() where TEntity : class where TRepository : IRepository<TEntity>;

        /// <summary>
        /// Obtiene el repositorio para la entidad especificada.
        /// </summary>
        /// <typeparam name="TEntity">Tipo de entidad.</typeparam>
        /// <returns>Instancia del repositorio genérico para la entidad.</returns>
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        /// <summary>
        /// Verifica si el contexto de la base de datos ha sido modificado.
        /// </summary>
        /// <returns>True si hay cambios pendientes, de lo contrario false.</returns>
        bool HasChanges();
    }
}
