using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class PrinterMonitoringService : IPrinterMonitoringService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrinterMonitoringService> _logger;
        private readonly IMapper _mapper;

        public PrinterMonitoringService(
            ApplicationDbContext context, 
            ILogger<PrinterMonitoringService> logger,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<Printer>> GetAllPrintersAsync()
        {
            try
            {
                return await _context.Printers.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las impresoras");
                throw;
            }
        }

        public async Task<Printer> GetPrinterByIdAsync(int id)
        {
            try
            {
                return await _context.Printers.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la impresora con ID: {id}");
                throw;
            }
        }

        public async Task<Printer> AddPrinterAsync(Printer printer)
        {
            try
            {
                // Validar que la impresora no exista ya
                var existingPrinter = await _context.Printers
                    .FirstOrDefaultAsync(p => p.IpAddress == printer.IpAddress || p.SerialNumber == printer.SerialNumber);

                if (existingPrinter != null)
                {
                    throw new InvalidOperationException("Ya existe una impresora con la misma dirección IP o número de serie.");
                }

                printer.CreatedAt = DateTime.UtcNow;
                printer.UpdatedAt = DateTime.UtcNow;
                printer.IsActive = true;
                
                _context.Printers.Add(printer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Impresora {printer.Name} agregada correctamente con ID {printer.Id}");
                
                return printer;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al agregar una nueva impresora");
                throw new ApplicationException("Error al guardar la impresora en la base de datos.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al agregar una nueva impresora");
                throw;
            }
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            try
            {
                _context.Entry(printer).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar la impresora con ID: {printer.Id}");
                throw;
            }
        }

        public async Task DeletePrinterAsync(int id)
        {
            try
            {
                var printer = await _context.Printers.FindAsync(id);
                if (printer != null)
                {
                    _context.Printers.Remove(printer);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar la impresora con ID: {id}");
                throw;
            }
        }

        public async Task CheckPrinterStatusAsync(int printerId)
        {
            try
            {
                var printer = await _context.Printers.FindAsync(printerId);
                if (printer == null)
                {
                    _logger.LogWarning($"Intento de verificación de estado para una impresora inexistente: ID {printerId}");
                    throw new KeyNotFoundException($"No se encontró la impresora con ID {printerId}");
                }

                // Actualizar la última vez que se verificó
                printer.LastChecked = DateTime.UtcNow;
                
                // Aquí iría la lógica para verificar el estado real de la impresora
                // Por ejemplo, usando SNMP o alguna otra API para verificar el estado
                
                // Simulación de verificación de estado
                var random = new Random();
                printer.Status = random.Next(0, 10) > 2 ? "Online" : "Offline";
                
                if (printer.Status == "Online")
                {
                    // Simular niveles de tóner
                    printer.BlackInkLevel = random.Next(10, 100);
                    printer.CyanInkLevel = random.Next(10, 100);
                    printer.MagentaInkLevel = random.Next(10, 100);
                    printer.YellowInkLevel = random.Next(10, 100);
                    
                    // Simular contador de páginas
                    printer.PageCount += random.Next(1, 10);
                }
                else
                {
                    // Si está offline, establecer niveles en 0
                    printer.BlackInkLevel = 0;
                    printer.CyanInkLevel = 0;
                    printer.MagentaInkLevel = 0;
                    printer.YellowInkLevel = 0;
                }
                
                printer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Estado de la impresora {printer.Name} (ID: {printer.Id}) actualizado a {printer.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar el estado de la impresora con ID {printerId}");
                throw new ApplicationException($"Error al verificar el estado de la impresora: {ex.Message}", ex);
            }
        }
    }
}
