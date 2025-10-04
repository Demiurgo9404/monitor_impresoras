using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Repositories
{
    public class PrinterRepository : IPrinterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrinterRepository> _logger;

        public PrinterRepository(ApplicationDbContext context, ILogger<PrinterRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Printer>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las impresoras");
                return await _context.Printers
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las impresoras");
                throw;
            }
        }

        public async Task<Printer?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo impresora con ID: {PrinterId}", id);
                return await _context.Printers
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la impresora con ID: {PrinterId}", id);
                throw;
            }
        }

        public async Task<Printer> AddAsync(Printer printer)
        {
            try
            {
                if (printer == null)
                    throw new ArgumentNullException(nameof(printer));

                _logger.LogInformation("Agregando nueva impresora: {PrinterName}", printer.Name);
                
                printer.CreatedAt = DateTime.UtcNow;
                printer.UpdatedAt = DateTime.UtcNow;
                
                _context.Printers.Add(printer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Impresora agregada exitosamente con ID: {PrinterId}", printer.Id);
                return printer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar la impresora: {PrinterName}", printer?.Name);
                throw;
            }
        }

        public async Task UpdateAsync(Printer printer)
        {
            try
            {
                if (printer == null)
                    throw new ArgumentNullException(nameof(printer));

                _logger.LogInformation("Actualizando impresora con ID: {PrinterId}", printer.Id);
                
                printer.UpdatedAt = DateTime.UtcNow;
                
                _context.Printers.Update(printer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Impresora actualizada exitosamente: {PrinterId}", printer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la impresora con ID: {PrinterId}", printer?.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando impresora con ID: {PrinterId}", id);
                
                var printer = await GetByIdAsync(id);
                if (printer == null)
                {
                    throw new KeyNotFoundException($"No se encontró la impresora con ID {id}");
                }

                _context.Printers.Remove(printer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Impresora eliminada exitosamente: {PrinterId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la impresora con ID: {PrinterId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _context.Printers.AnyAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si existe la impresora con ID: {PrinterId}", id);
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en la base de datos");
                throw;
            }
        }
    }
}
