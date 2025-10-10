using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces;
using QOPIQ.Application.Interfaces.MultiTenancy;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Data;

namespace QOPIQ.Infrastructure.Repositories
{
    public class PrinterRepository : IPrinterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<PrinterRepository> _logger;

        public PrinterRepository(
            ApplicationDbContext context,
            ITenantAccessor tenantAccessor,
            ILogger<PrinterRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetTenantId()
        {
            if (!_tenantAccessor.HasTenant)
            {
                _logger.LogError("No se pudo determinar el Tenant ID para la operación");
                throw new InvalidOperationException("No se pudo determinar el Tenant ID para la operación");
            }

            var tenantId = _tenantAccessor.TenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                _logger.LogError("El Tenant ID no puede ser nulo o vacío");
                throw new InvalidOperationException("El Tenant ID no puede ser nulo o vacío");
            }

            return tenantId;
        }

        public async Task<Printer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("El ID no puede estar vacío", nameof(id));
            }

            var tenantId = GetTenantId();
            _logger.LogDebug("Buscando Impresora con ID: {Id} para el tenant: {TenantId}", id, tenantId);
            
            return await _context.Printers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
        }

        public async Task<IEnumerable<Printer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = GetTenantId();
            _logger.LogDebug("Obteniendo todas las Impresoras para el tenant: {TenantId}", tenantId);
            
            return await _context.Printers
                .AsNoTracking()
                .Where(p => p.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Printer printer, CancellationToken cancellationToken = default)
        {
            if (printer == null) 
            {
                throw new ArgumentNullException(nameof(printer));
            }

            var tenantId = GetTenantId();
            _logger.LogDebug("Agregando nueva Impresora para el tenant: {TenantId}", tenantId);
            
            // Asegurarse de que el TenantId de la Impresora coincida con el del contexto
            if (!string.IsNullOrEmpty(printer.TenantId) && printer.TenantId != tenantId)
            {
                _logger.LogWarning("Intento de agregar Impresora con TenantId diferente al del contexto. TenantId de la Impresora: {PrinterTenantId}, TenantId del contexto: {ContextTenantId}", 
                    printer.TenantId, tenantId);
                throw new InvalidOperationException("No se puede agregar una Impresora con un TenantId diferente al del contexto actual");
            }
            
            printer.TenantId = tenantId;
            await _context.Printers.AddAsync(printer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Impresora agregada con ID: {Id} para el tenant: {TenantId}", printer.Id, tenantId);
        }

        public async Task UpdateAsync(Printer printer, CancellationToken cancellationToken = default)
        {
            if (printer == null)
            {
                throw new ArgumentNullException(nameof(printer));
            }

            var tenantId = GetTenantId();
            
            // Verificar que la impresora pertenezca al tenant actual
            var existingPrinter = await _context.Printers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == printer.Id && p.TenantId == tenantId, cancellationToken);
                
            if (existingPrinter == null)
            {
                _logger.LogWarning("No se encontró la impresora con ID: {Id} para el tenant: {TenantId}", printer.Id, tenantId);
                throw new InvalidOperationException("La impresora especificada no existe o no pertenece a este tenant");
            }
            
            // Asegurar que el TenantId no se modifique
            printer.TenantId = tenantId;
            
            _logger.LogInformation("Actualizando impresora con ID: {Id} para el tenant: {TenantId}", printer.Id, tenantId);
            
            _context.Printers.Update(printer);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Impresora actualizada exitosamente con ID: {Id}", printer.Id);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("El ID no puede estar vacío", nameof(id));
            }

            var tenantId = GetTenantId();
            
            _logger.LogDebug("Eliminando Impresora con ID: {Id} para el tenant: {TenantId}", id, tenantId);
            
            var printer = await _context.Printers
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
                
            if (printer == null)
            {
                _logger.LogWarning("No se encontró la impresora con ID: {Id} para el tenant: {TenantId}", id, tenantId);
                throw new InvalidOperationException("La impresora especificada no existe o no pertenece a este tenant");
            }
            
            _context.Printers.Remove(printer);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Impresora eliminada con ID: {Id} para el tenant: {TenantId}", id, tenantId);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return false;
            }

            var tenantId = GetTenantId();
            _logger.LogDebug("Verificando existencia de Impresora con ID: {Id} para el tenant: {TenantId}", id, tenantId);
            
            return await _context.Printers
                .AsNoTracking()
                .AnyAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
        }
    }
}

