using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using QOPIQ.API.Hubs;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Models;

namespace QOPIQ.Application.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrinterService> _logger;
        private readonly IHubContext<PrinterHub> _hubContext;

        public PrinterService(
            IUnitOfWork unitOfWork, 
            ILogger<PrinterService> logger,
            IHubContext<PrinterHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<Printer>> GetAllPrintersAsync()
        {
            try
            {
                return await _unitOfWork.Printers.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las impresoras");
                throw;
            }
        }

        public async Task<Printer> AddPrinterAsync(PrinterCreateDto printerDto)
        {
            try
            {
                var printer = new Printer
                {
                    Id = Guid.NewGuid(),
                    Name = printerDto.Name,
                    IpAddress = printerDto.IpAddress,
                    Model = printerDto.Model,
                    Status = PrinterStatus.Offline.ToString(),
                    LastChecked = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Printers.AddAsync(printer);
                await _unitOfWork.SaveChangesAsync();

                // Notificar a los clientes sobre la nueva impresora
                await NotifyPrinterStatusChange(printer.Id, printer.Status);

                return printer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar una nueva impresora");
                throw;
            }
        }

        public async Task<bool> CheckPrinterStatusAsync(string ipAddress)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, 1000);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar el estado de la impresora con IP {ipAddress}");
                return false;
            }
        }
        
        private async Task NotifyPrinterStatusChange(Guid printerId, string status)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceivePrinterStatus", printerId.ToString(), status);
                _logger.LogInformation($"Notificación de estado enviada para la Impressora {printerId}: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar notificación de estado para la Impressora {printerId}");
            }
        }

        public async Task<PrinterStatusDto> GetPrinterStatusAsync(Guid printerId)
        {
            try
            {
                var printer = await _unitOfWork.Printers.GetByIdAsync(printerId);
                if (printer == null)
                    throw new KeyNotFoundException($"No se encontró la impresora con ID {printerId}");

                var isOnline = await CheckPrinterStatusAsync(printer.IpAddress);
                var status = isOnline ? PrinterStatus.Online.ToString() : PrinterStatus.Offline.ToString();
                var lastChecked = DateTime.UtcNow;

                // Actualizar el estado de la impresora
                printer.Status = status;
                printer.LastChecked = lastChecked;
                await _unitOfWork.SaveChangesAsync();

                // Notificar el cambio de estado
                await NotifyPrinterStatusChange(printerId, status);

                var metrics = new Dictionary<string, string>
                {
                    { "Modelo", printer.Model },
                    { "Ubicación", printer.Location ?? "No especificada" },
                    { "Última verificación", lastChecked.ToString("g") },
                    { "Estado", status }
                };

                return new PrinterStatusDto
                {
                    PrinterId = printerId,
                    Status = status,
                    LastChecked = lastChecked,
                    IsOnline = isOnline,
                    Message = $"La impresora está {status}",
                    Name = printer.Name,
                    IpAddress = printer.IpAddress,
                    StatusMessage = status == PrinterStatus.Online.ToString() ? "La impresora está respondiendo" : "No se pudo conectar a la impresora",
                    Metrics = metrics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el estado de la impresora {printerId}");
                throw;
            }
        }

        public async Task<IEnumerable<Printer>> ScanNetworkForPrintersAsync()
        {
            var foundPrinters = new List<Printer>();
            // Implementar escaneo de red real aquí
            // Esto es solo un ejemplo básico
            return await Task.FromResult(foundPrinters);
        }

        public async Task<PrinterStatsDto> GetPrinterStatsAsync()
        {
            try
            {
                var printers = await _unitOfWork.Printers.GetAllAsync();
                var onlinePrinters = printers.Count(p => p.IsOnline);
                var offlinePrinters = printers.Count - onlinePrinters;
                var needsMaintenance = printers.Count(p => p.StatusMessage?.Contains("mantenimiento", StringComparison.OrdinalIgnoreCase) == true);
                var lowOnSupplies = printers.Count(p => p.StatusMessage?.Contains("bajo", StringComparison.OrdinalIgnoreCase) == true);

                var statusCount = printers
                    .GroupBy(p => p.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                var modelDistribution = printers
                    .GroupBy(p => p.Model ?? "Desconocido")
                    .ToDictionary(g => g.Key, g => g.Count());

                return new PrinterStatsDto
                {
                    TotalPrinters = printers.Count(),
                    OnlinePrinters = onlinePrinters,
                    OfflinePrinters = offlinePrinters,
                    NeedsMaintenance = needsMaintenance,
                    LowOnSupplies = lowOnSupplies,
                    StatusCount = statusCount,
                    ModelDistribution = modelDistribution
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de impresoras");
                throw;
            }
        }
    }
}
