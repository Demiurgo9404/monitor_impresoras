using Microsoft.EntityFrameworkCore.Storage;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Infrastructure.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Infrastructure.Data
{
    /// <summary>
    /// Implementación de la unidad de trabajo que maneja transacciones y guardado de cambios
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed = false;

        public IPrinterRepository Printers { get; }

        public UnitOfWork(ApplicationDbContext context, IPrinterRepository printerRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Printers = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            throw new InvalidOperationException("Ya existe una transacción en curso");

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No hay ninguna transacción activa para confirmar");

            try
            {
                await SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                return; // No hay transacción que revertir

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _currentTransaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_currentTransaction != null)
                    {
                        await _currentTransaction.DisposeAsync();
                    }
                    await _context.DisposeAsync();
                }
                _disposed = true;
            }
        }
    }
}
