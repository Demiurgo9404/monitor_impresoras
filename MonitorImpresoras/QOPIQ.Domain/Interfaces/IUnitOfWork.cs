using System;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces
{
    /// <summary>
    /// Define la interfaz para la unidad de trabajo que maneja transacciones y guardado de cambios
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Guarda todos los cambios realizados en el contexto de la base de datos
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Inicia una nueva transacción
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Confirma la transacción actual
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Revierte la transacción actual
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
