using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Services.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.Application.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly IPrinterRepository _printerRepository;
        private readonly ILogger<PrinterService> _logger;

        public PrinterService(IPrinterRepository printerRepository, ILogger<PrinterService> logger)
        {
            _printerRepository = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<string>> GetPrintersAsync()
        {
            try
            {
                var printers = await _printerRepository.GetAllAsync();
                var printerNames = printers.Select(p => p.Name).ToList();

                _logger.LogInformation("Retrieved {Count} printers", printerNames.Count);
                return printerNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving printers");
                throw new ApplicationException("Failed to retrieve printers", ex);
            }
        }

        public async Task<string> GetPrinterStatusAsync(Guid printerId)
        {
            try
            {
                var printer = await _printerRepository.GetByIdAsync(printerId);
                if (printer == null)
                {
                    throw new KeyNotFoundException($"Printer with ID {printerId} not found");
                }

                var status = printer.IsOnline ? "Online" : "Offline";
                _logger.LogInformation("Printer {PrinterName} status: {Status}", printer.Name, status);

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer status for {PrinterId}", printerId);
                throw new ApplicationException($"Failed to get printer status for {printerId}", ex);
            }
        }
    }
}
