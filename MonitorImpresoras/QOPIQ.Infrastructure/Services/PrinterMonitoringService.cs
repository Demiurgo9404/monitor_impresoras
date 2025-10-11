using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Domain.Interfaces.Services;

namespace QOPIQ.Infrastructure.Services
{
    public class PrinterMonitoringService : IPrinterMonitoringService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Printer> _printerRepository;
        private readonly ISnmpService _snmpService;

        public PrinterMonitoringService(IUnitOfWork unitOfWork, ISnmpService snmpService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _printerRepository = unitOfWork.GetRepository<Printer>();
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
        }

        public async Task<IEnumerable<Printer>> GetAllPrintersAsync()
        {
            return await _printerRepository.GetAllAsync();
        }

        public async Task<Printer?> GetPrinterByIdAsync(Guid id)
        {
            return await _printerRepository.GetByIdAsync(id);
        }

        public async Task AddPrinterAsync(Printer printer)
        {
            if (printer == null)
                throw new ArgumentNullException(nameof(printer));

            await _printerRepository.AddAsync(printer);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            if (printer == null)
                throw new ArgumentNullException(nameof(printer));

            _printerRepository.Update(printer);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeletePrinterAsync(Guid id)
        {
            var printer = await _printerRepository.GetByIdAsync(id);
            if (printer != null)
            {
                _printerRepository.Delete(printer);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> TestPrinterConnectionAsync(string ipAddress)
        {
            // Simula una prueba de conexión
            await Task.Delay(100);
            return true;
        }

        public async Task<PrinterStatus> GetPrinterStatusAsync(Guid id)
        {
            // Simulación de estado
            await Task.Delay(50);
            return PrinterStatus.Online;
        }

        public async Task MonitorPrintersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Start a new transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var printers = await _printerRepository.GetAllAsync();
                foreach (var printer in printers)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        // Get printer status via SNMP
                        var statusText = await _snmpService.GetPrinterStatusAsync(printer.IpAddress);
                        
                        // Convert status text to enum
                        var status = statusText.ToLower() switch
                        {
                            "online" => PrinterStatus.Online,
                            "offline" => PrinterStatus.Offline,
                            "error" => PrinterStatus.Error,
                            "warning" => PrinterStatus.Warning,
                            "maintenance" => PrinterStatus.Maintenance,
                            _ => PrinterStatus.Offline
                        };
                        
                        // Update printer status and last update time
                        printer.Status = status;
                        printer.LastStatusUpdate = DateTime.UtcNow;
                        printer.IsOnline = status != PrinterStatus.Offline && status != PrinterStatus.Error;
                        
                        // Update printer in database
                        _printerRepository.Update(printer);
                    }
                    catch (Exception ex)
                    {
                        // Update printer status to error if there's an issue
                        printer.Status = PrinterStatus.Error;
                        printer.StatusMessage = $"Error checking status: {ex.Message}";
                        printer.LastStatusUpdate = DateTime.UtcNow;
                        printer.IsOnline = false;
                        
                        _printerRepository.Update(printer);
                        
                        // Log the error
                        Console.WriteLine($"Error updating printer {printer.Id} at {printer.IpAddress}: {ex.Message}");
                    }
                }

                // Save all changes in a single transaction
                int changes = await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Commit the transaction if we get here without exceptions
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                
                // Log the monitoring result
                Console.WriteLine($"Successfully monitored {printers.Count()} printers. {changes} changes saved.");
            }
            catch (Exception ex)
            {
                // Rollback the transaction on error
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                
                // Log the error
                Console.WriteLine($"Error in MonitorPrintersAsync: {ex.Message}");
                
                // Re-throw to allow the caller to handle the error
                throw new InvalidOperationException("Failed to monitor printers. See inner exception for details.", ex);
            }
        }
    }
}
