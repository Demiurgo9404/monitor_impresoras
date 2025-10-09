using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Infrastructure.Services
{
    public class PrinterMonitoringService : IPrinterMonitoringService
    {
        private readonly IPrinterRepository _printerRepository;
        private readonly ISnmpService _snmpService;
        private readonly ILogger<PrinterMonitoringService> _logger;

        public PrinterMonitoringService(
            IPrinterRepository printerRepository,
            ISnmpService snmpService,
            ILogger<PrinterMonitoringService> logger)
        {
            _printerRepository = printerRepository;
            _snmpService = snmpService;
            _logger = logger;
        }

        public async Task<List<Printer>> GetAllPrintersAsync()
        {
            try
            {
                return await _printerRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all printers");
                return new List<Printer>();
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
                _logger.LogError(ex, "Error getting printer {PrinterId}", id);
                return null;
            }
        }

        public async Task<Printer> AddPrinterAsync(Printer printer)
        {
            try
            {
                return await _printerRepository.AddAsync(printer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding printer {PrinterName}", printer.Name);
                throw;
            }
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            try
            {
                await _printerRepository.UpdateAsync(printer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating printer {PrinterId}", printer.Id);
                throw;
            }
        }

        public async Task DeletePrinterAsync(Guid id)
        {
            try
            {
                await _printerRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting printer {PrinterId}", id);
                throw;
            }
        }

        public async Task<bool> TestPrinterConnectionAsync(string ipAddress)
        {
            try
            {
                return await _snmpService.TestConnectionAsync(ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection to {IpAddress}", ipAddress);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetPrinterStatusAsync(Guid printerId)
        {
            try
            {
                var printer = await _printerRepository.GetByIdAsync(printerId);
                if (printer == null)
                {
                    return new Dictionary<string, object> { ["error"] = "Printer not found" };
                }

                return await _snmpService.GetPrinterInfoAsync(printer.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for printer {PrinterId}", printerId);
                return new Dictionary<string, object> { ["error"] = ex.Message };
            }
        }
    }
}

