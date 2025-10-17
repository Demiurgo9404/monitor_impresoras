using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Options;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Enums;
using QOPIQ.Infrastructure.Configuration;

// Use fully qualified names to avoid ambiguity
using IPrinterMonitoringService = QOPIQ.Application.Interfaces.IPrinterMonitoringService;
using IPrinterRepository = QOPIQ.Domain.Interfaces.Repositories.IPrinterRepository;
using ISnmpService = QOPIQ.Domain.Interfaces.Services.ISnmpService;

namespace QOPIQ.Infrastructure.Services
{
    public class PrinterMonitoringService : IPrinterMonitoringService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPrinterRepository _printerRepository;
        private readonly ISnmpService _snmpService;
        private readonly SnmpOptions _options;

        public PrinterMonitoringService(
            IUnitOfWork unitOfWork, 
            ISnmpService snmpService,
            IOptions<SnmpOptions> options)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _printerRepository = unitOfWork.Printers ?? throw new ArgumentNullException(nameof(unitOfWork.Printers));
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<List<Printer>> GetAllPrintersAsync()
        {
            var printers = await _printerRepository.GetAllAsync(CancellationToken.None);
            return new List<Printer>(printers);
        }

        public async Task<Printer?> GetPrinterByIdAsync(Guid id)
        {
            return await _printerRepository.GetByIdAsync(id, CancellationToken.None);
        }

        public async Task<Printer> AddPrinterAsync(Printer printer)
        {
            if (printer == null) throw new ArgumentNullException(nameof(printer));
            
            // Check if a printer with the same IP already exists
            var existingPrinter = await _printerRepository.GetByIpAddressAsync(printer.IpAddress, CancellationToken.None);
            if (existingPrinter != null)
            {
                throw new InvalidOperationException($"A printer with IP {printer.IpAddress} already exists");
            }

            await _printerRepository.AddAsync(printer, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            return printer;
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            if (printer == null) throw new ArgumentNullException(nameof(printer));
            
            var existingPrinter = await _printerRepository.GetByIdAsync(printer.Id, CancellationToken.None);
            if (existingPrinter == null)
            {
                throw new KeyNotFoundException($"Printer with ID {printer.Id} not found");
            }

            // Check if IP is being changed and if it's already in use
            if (existingPrinter.IpAddress != printer.IpAddress)
            {
                var printerWithSameIp = await _printerRepository.GetByIpAddressAsync(printer.IpAddress, CancellationToken.None);
                if (printerWithSameIp != null)
                {
                    throw new InvalidOperationException($"A printer with IP {printer.IpAddress} already exists");
                }
            }

            // Update properties
            existingPrinter.Name = printer.Name;
            existingPrinter.Model = printer.Model;
            existingPrinter.IpAddress = printer.IpAddress;
            existingPrinter.Location = printer.Location;
            existingPrinter.DepartmentId = printer.DepartmentId;
            existingPrinter.IsActive = printer.IsActive;
            existingPrinter.UpdatedAt = DateTime.UtcNow;

            _printerRepository.Update(existingPrinter);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeletePrinterAsync(Guid id)
        {
            var printer = await _printerRepository.GetByIdAsync(id, CancellationToken.None);
            if (printer != null)
            {
                _printerRepository.Remove(printer);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
        }

        public async Task<bool> TestPrinterConnectionAsync(string ipAddress)
        {
            try
            {
                var status = await _snmpService.GetPrinterStatusAsync(ipAddress, _options.Community);
                return !string.IsNullOrEmpty(status);
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetPrinterStatusAsync(Guid printerId)
        {
            var printer = await _printerRepository.GetByIdAsync(printerId, CancellationToken.None);
            if (printer == null)
                throw new KeyNotFoundException($"Printer with ID {printerId} not found");

            var status = new Dictionary<string, object>
            {
                ["printerId"] = printer.Id,
                ["name"] = printer.Name,
                ["status"] = printer.Status.ToString(),
                ["isOnline"] = printer.IsOnline,
                ["lastStatusUpdate"] = printer.UpdatedAt,
                ["ipAddress"] = printer.IpAddress
            };

            // Add SNMP information if available
            if (printer.IsOnline && !string.IsNullOrEmpty(printer.IpAddress))
            {
                try
                {
                    var snmpInfo = await _snmpService.GetPrinterInfoAsync(printer.IpAddress, _options.Community);
                    foreach (var kvp in snmpInfo)
                    {
                        status[kvp.Key] = kvp.Value;
                    }
                }
                catch (Exception ex)
                {
                    status["snmpError"] = ex.Message;
                }
            }

            return status;
        }
    }
}
