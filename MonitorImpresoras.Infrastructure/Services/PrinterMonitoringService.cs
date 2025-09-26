using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Interfaces.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class PrinterMonitoringService : IPrinterMonitoringService, IDisposable
    {
        private readonly IPrinterRepository _printerRepository;
        private readonly ISnmpService _snmpService;
        private readonly IWindowsPrinterService _windowsPrinterService;
        private readonly ILogger<PrinterMonitoringService> _logger;
        private bool _disposed = false;

        public PrinterMonitoringService(
            IPrinterRepository printerRepository,
            ISnmpService snmpService,
            IWindowsPrinterService windowsPrinterService,
            ILogger<PrinterMonitoringService> logger)
        {
            _printerRepository = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
            _windowsPrinterService = windowsPrinterService ?? throw new ArgumentNullException(nameof(windowsPrinterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PrinterStatus> GetPrinterStatusAsync(Printer printer)
        {
            if (printer == null)
                throw new ArgumentNullException(nameof(printer));

            var status = new PrinterStatus();
            
            try
            {
                // Verificar si la impresora está en línea
                bool isOnline = await _snmpService.IsPrinterOnlineAsync(IPAddress.Parse(printer.IpAddress));
                status.IsOnline = isOnline;
                
                if (isOnline)
                {
                    // Obtener métricas SNMP
                    var metrics = await _snmpService.GetPrinterMetricsAsync(IPAddress.Parse(printer.IpAddress));
                    status.Metrics = metrics;

                    // Si la impresora es local, intentar obtener el contador de páginas
                    if (printer.IsLocalPrinter && !string.IsNullOrEmpty(printer.Name))
                    {
                        try
                        {
                            status.PageCount = await _windowsPrinterService.GetPrinterPageCountAsync(printer.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al obtener el contador de páginas para la impresora {PrinterName}", printer.Name);
                        }
                    }

                    // Actualizar estado basado en las métricas
                    if (metrics.TryGetValue("Estado", out string estado))
                    {
                        status.Status = estado;
                    }
                    else if (metrics.Any())
                    {
                        status.Status = "En línea";
                    }
                }
                else
                {
                    status.Status = "Fuera de línea";
                }

                // Actualizar la base de datos
                await UpdatePrinterInDatabase(printer, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al monitorear la impresora {PrinterName} ({IpAddress})", printer.Name, printer.IpAddress);
                status.Status = $"Error: {ex.Message}";
                status.IsOnline = false;
            }

            return status;
        }

        public async Task UpdateAllPrintersStatusAsync()
        {
            try
            {
                var printers = await _printerRepository.GetAllAsync();
                var tasks = printers.Select(printer => GetPrinterStatusAsync(printer));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el estado de todas las impresoras");
                throw;
            }
        }

        private async Task UpdatePrinterInDatabase(Printer printer, PrinterStatus status)
        {
            try
            {
                printer.IsOnline = status.IsOnline;
                printer.Status = status.Status;
                printer.LastChecked = DateTime.UtcNow;

                // Actualizar niveles de consumibles si están disponibles
                if (status.Metrics != null)
                {
                    var consumableLevels = new Dictionary<int, int>();
                    
                    // Procesar métricas de tóner
                    if (status.Metrics.TryGetValue("NivelToner", out string nivelTonerStr) &&
                        int.TryParse(nivelTonerStr, out int nivelToner))
                    {
                        // Asumimos que el primer consumible es el tóner
                        var toner = await _printerRepository.GetPrinterWithConsumablesByIdAsync(printer.Id);
                        var primerConsumible = toner?.Consumables.FirstOrDefault();
                        if (primerConsumible != null)
                        {
                            consumableLevels[primerConsumible.Id] = nivelToner;
                        }
                    }

                    if (consumableLevels.Any())
                    {
                        await _printerRepository.UpdateConsumableLevelsAsync(printer.Id, consumableLevels);
                    }
                }

                await _printerRepository.UpdatePrinterStatusAsync(printer.Id, status.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la impresora {PrinterId} en la base de datos", printer.Id);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Liberar recursos administrados
                    if (_snmpService is IDisposable snmpDisposable)
                        snmpDisposable.Dispose();
                    
                    if (_windowsPrinterService is IDisposable wmiDisposable)
                        wmiDisposable.Dispose();
                }
                _disposed = true;
            }
        }

        ~PrinterMonitoringService()
        {
            Dispose(false);
        }
    }
}
