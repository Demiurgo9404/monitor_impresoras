using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Domain.Interfaces.Services;

namespace QOPIQ.Infrastructure.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly IPrinterRepository _printerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrinterService> _logger;

        public PrinterService(
            IPrinterRepository printerRepository,
            IUnitOfWork unitOfWork,
            ILogger<PrinterService> logger)
        {
            _printerRepository = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Printer>> GetAllPrintersAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las impresoras");
                return await _printerRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las impresoras");
                throw;
            }
        }

        public async Task<Printer?> GetPrinterByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo impresora con ID: {PrinterId}", id);
                return await _printerRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la impresora con ID: {PrinterId}", id);
                throw;
            }
        }

        public async Task<Printer> CreatePrinterAsync(Printer printer)
        {
            try
            {
                _logger.LogInformation("Creando nueva impresora: {PrinterName}", printer.Name);
                await _printerRepository.AddAsync(printer);
                await _unitOfWork.SaveChangesAsync();
                return printer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la impresora: {PrinterName}", printer.Name);
                throw;
            }
        }

        public async Task<bool> UpdatePrinterAsync(Printer printer)
        {
            try
            {
                _logger.LogInformation("Actualizando impresora con ID: {PrinterId}", printer.Id);
                await _printerRepository.UpdateAsync(printer, CancellationToken.None);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la impresora con ID: {PrinterId}", printer.Id);
                throw;
            }
        }

        public async Task<bool> DeletePrinterAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando impresora con ID: {PrinterId}", id);
                var printer = await _printerRepository.GetByIdAsync(id);
                if (printer == null)
                {
                    _logger.LogWarning("No se encontró la impresora con ID: {PrinterId} para eliminar", id);
                    return false;
                }

                await _printerRepository.RemoveAsync(printer, CancellationToken.None);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la impresora con ID: {PrinterId}", id);
                throw;
            }
        }

        public async Task<bool> CheckPrinterStatusAsync(string ipAddress)
        {
            try
            {
                _logger.LogInformation("Verificando estado de la impresora en {IP}", ipAddress);
                // Implementación básica de verificación de estado
                // En una implementación real, esto podría usar SNMP u otro protocolo
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(ipAddress, 2000); // Timeout de 2 segundos
                return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de la impresora en {IP}", ipAddress);
                return false;
            }
        }
    }
}
