using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;
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
                return await _printerRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la impresora con ID: {id}");
                throw;
            }
        }

        public async Task<Printer> CreatePrinterAsync(Printer printer)
        {
            try
            {
                await _printerRepository.AddAsync(printer);
                await _unitOfWork.SaveChangesAsync();
                return printer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la impresora");
                throw;
            }
        }

        public async Task<bool> UpdatePrinterAsync(Printer printer)
        {
            try
            {
                _printerRepository.Update(printer);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar la impresora con ID: {printer.Id}");
                return false;
            }
        }

        public async Task<bool> DeletePrinterAsync(Guid id)
        {
            try
            {
                var printer = await _printerRepository.GetByIdAsync(id);
                if (printer == null)
                    return false;

                _printerRepository.Remove(printer);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar la impresora con ID: {id}");
                return false;
            }
        }
    }
}
