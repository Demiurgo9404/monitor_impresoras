using System;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Interfaces.Repositories;

namespace QOPIQ.Domain.Interfaces
{
    /// <summary>
    /// Interfaz que define la unidad de trabajo para el patrón Repository.
    /// Coordina el trabajo de múltiples repositorios creando una única transacción de base de datos.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Repositorio para operaciones de impresoras.
        /// </summary>
        IPrinterRepository Printers { get; }

        /// <summary>
        /// Repositorio para operaciones de usuarios.
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Repositorio para operaciones de tokens de actualización.
        /// </summary>
        IRefreshTokenRepository RefreshTokens { get; }

        /// <summary>
        /// Repositorio para operaciones de suscripciones.
        /// </summary>
        ISubscriptionRepository Subscriptions { get; }

        /// <summary>
        /// Guarda todos los cambios realizados en el contexto de la base de datos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Número de entidades afectadas.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Inicia una nueva transacción de base de datos.
        /// </summary>
        /// <returns>Tarea asíncrona.</returns>
        Task BeginTransactionAsync();
        
        /// <summary>
        /// Confirma la transacción actual.
        /// </summary>
        /// <returns>Tarea asíncrona.</returns>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Deshace la transacción actual.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Verifica si hay una transacción activa.
        /// </summary>
        /// <returns>True si hay una transacción activa, de lo contrario false.</returns>
        bool HasActiveTransaction();

        /// <summary>
        /// Intenta completar la unidad de trabajo de manera atómica.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>True si se completó con éxito, de lo contrario false.</returns>
        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }
}
