using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Infrastructure.Data.Repositories;

namespace QOPIQ.Infrastructure.Data
{
    /// <summary>
    /// Implementación de la unidad de trabajo que coordina múltiples repositorios
    /// y gestiona transacciones de base de datos.
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        private IPrinterRepository _printers;
        private IUserRepository _users;
        private IRefreshTokenRepository _refreshTokens;
        private ISubscriptionRepository _subscriptions;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UnitOfWork"/>
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public IPrinterRepository Printers => _printers ??= new PrinterRepository(_context);
        
        /// <inheritdoc />
        public IUserRepository Users => _users ??= new UserRepository(_context);
        
        /// <inheritdoc />
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
        
        /// <inheritdoc />
        public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Ya hay una transacción activa");
            }
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay una transacción activa para confirmar");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                return;
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        /// <inheritdoc />
        public bool HasActiveTransaction()
        {
            return _transaction != null;
        }

        /// <inheritdoc />
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion

        #region IAsyncDisposable Implementation

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        await _transaction.DisposeAsync();
                    }
                    await _context.DisposeAsync();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}