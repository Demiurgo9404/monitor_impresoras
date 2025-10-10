using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Repositories;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Repositories;

namespace QOPIQ.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private bool _disposed = false;
        private IPrinterRepository? _printerRepository;

        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IPrinterRepository PrinterRepository => 
            _printerRepository ??= new PrinterRepository(_context);

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IEntity
        {
            // Implementación genérica de repositorio
            return new Repository<TEntity>(_context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Guardando cambios en la base de datos");
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en la base de datos");
                throw;
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.BeginTransactionAsync(cancellationToken);
            _logger.LogInformation("Iniciando transacción");
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Database.CommitTransactionAsync(cancellationToken);
                _logger.LogInformation("Transacción confirmada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar la transacción");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.RollbackTransactionAsync(cancellationToken);
            _logger.LogWarning("Transacción revertida");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
