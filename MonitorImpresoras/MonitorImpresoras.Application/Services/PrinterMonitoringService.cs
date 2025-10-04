using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Services
{
    public class PrinterMonitoringService : IPrinterMonitoringService
    {
        private readonly IPrinterRepository _printerRepository;
        private readonly IWindowsPrinterService _windowsPrinterService;
        private readonly ISnmpService _snmpService;
        private readonly ILogger<PrinterMonitoringService> _logger;

        public PrinterMonitoringService(
            IPrinterRepository printerRepository,
            IWindowsPrinterService windowsPrinterService,
            ISnmpService snmpService,
            ILogger<PrinterMonitoringService> logger)
        {
            _printerRepository = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
            _windowsPrinterService = windowsPrinterService ?? throw new ArgumentNullException(nameof(windowsPrinterService));
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
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
                var printer = await _printerRepository.GetByIdAsync(id);
                
                if (printer == null)
                {
                    _logger.LogWarning("No se encontró la impresora con ID: {PrinterId}", id);
                    throw new KeyNotFoundException($"No se encontró la impresora con ID {id}");
                }

                return printer;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la impresora con ID: {PrinterId}", id);
                throw;
            }
        }

        public async Task<Printer> AddPrinterAsync(Printer printer)
        {
            try
            {
                if (printer == null)
                    throw new ArgumentNullException(nameof(printer));

                _logger.LogInformation("Agregando nueva impresora: {PrinterName}", printer.Name);

                // Validar que no exista una impresora con la misma IP
                var existingPrinters = await _printerRepository.GetAllAsync();
                if (existingPrinters.Any(p => p.IpAddress == printer.IpAddress))
                {
                    throw new InvalidOperationException($"Ya existe una impresora con la dirección IP {printer.IpAddress}");
                }

                // Establecer valores por defecto
                printer.CreatedAt = DateTime.UtcNow;
                printer.UpdatedAt = DateTime.UtcNow;
                printer.Status = "Unknown";
                printer.IsOnline = false;

                var addedPrinter = await _printerRepository.AddAsync(printer);
                
                // Verificar el estado inicial de la impresora
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await CheckPrinterStatusAsync(addedPrinter.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo verificar el estado inicial de la impresora {PrinterId}", addedPrinter.Id);
                    }
                });

                return addedPrinter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar la impresora: {PrinterName}", printer?.Name);
                throw;
            }
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            try
            {
                if (printer == null)
                    throw new ArgumentNullException(nameof(printer));

                _logger.LogInformation("Actualizando impresora con ID: {PrinterId}", printer.Id);

                // Verificar que la impresora existe
                var existingPrinter = await _printerRepository.GetByIdAsync(printer.Id);
                if (existingPrinter == null)
                {
                    throw new KeyNotFoundException($"No se encontró la impresora con ID {printer.Id}");
                }

                printer.UpdatedAt = DateTime.UtcNow;
                await _printerRepository.UpdateAsync(printer);
                
                _logger.LogInformation("Impresora actualizada exitosamente: {PrinterId}", printer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la impresora con ID: {PrinterId}", printer?.Id);
                throw;
            }
        }

        public async Task DeletePrinterAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando impresora con ID: {PrinterId}", id);
                await _printerRepository.DeleteAsync(id);
                _logger.LogInformation("Impresora eliminada exitosamente: {PrinterId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la impresora con ID: {PrinterId}", id);
                throw;
            }
        }

        public async Task CheckPrinterStatusAsync(Guid printerId)
        {
            try
            {
                _logger.LogInformation("Verificando estado de la impresora con ID: {PrinterId}", printerId);

                var printer = await _printerRepository.GetByIdAsync(printerId);
                if (printer == null)
                {
                    throw new KeyNotFoundException($"No se encontró la impresora con ID {printerId}");
                }

                // Verificar estado usando el servicio apropiado
                if (printer.IsLocalPrinter)
                {
                    await CheckLocalPrinterStatusAsync(printer);
                }
                else
                {
                    await CheckNetworkPrinterStatusAsync(printer);
                }

                printer.LastChecked = DateTime.UtcNow;
                await _printerRepository.UpdateAsync(printer);

                _logger.LogInformation("Estado de la impresora {PrinterId} verificado: {Status}", printerId, printer.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de la impresora con ID: {PrinterId}", printerId);
                throw;
            }
        }

        public async Task CheckAllPrintersStatusAsync()
        {
            try
            {
                _logger.LogInformation("Verificando estado de todas las impresoras");

                var printers = await _printerRepository.GetAllAsync();
                var tasks = new List<Task>();

                foreach (var printer in printers)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await CheckPrinterStatusAsync(printer.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error al verificar el estado de la impresora {PrinterId}", printer.Id);
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                _logger.LogInformation("Verificación de estado completada para todas las impresoras");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de todas las impresoras");
                throw;
            }
        }

        private async Task CheckLocalPrinterStatusAsync(Printer printer)
        {
            try
            {
                var status = await _windowsPrinterService.GetPrinterStatusAsync(printer.Name);
                
                printer.Status = status.ContainsKey("Status") ? status["Status"].ToString() ?? "Unknown" : "Unknown";
                printer.IsOnline = !printer.Status.Contains("Error") && !printer.Status.Contains("Offline");
                printer.LastError = status.ContainsKey("ErrorMessage") ? status["ErrorMessage"].ToString() ?? "" : "";
                
                if (status.ContainsKey("PageCount") && int.TryParse(status["PageCount"].ToString(), out int pageCount))
                {
                    printer.PageCount = pageCount;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar impresora local {PrinterName}", printer.Name);
                printer.Status = "Error";
                printer.IsOnline = false;
                printer.LastError = ex.Message;
            }
        }

        private async Task CheckNetworkPrinterStatusAsync(Printer printer)
        {
            try
            {
                var ipAddress = System.Net.IPAddress.Parse(printer.IpAddress);
                var isOnline = await _snmpService.IsPrinterOnlineAsync(ipAddress, printer.SnmpPort ?? 161);
                printer.IsOnline = isOnline;
                printer.Status = isOnline ? "Online" : "Offline";

                if (isOnline)
                {
                    // Obtener contador de páginas via SNMP
                    try
                    {
                        var pageCount = await _snmpService.GetPageCountAsync(printer.Name, printer.IpAddress, printer.SnmpPort ?? 161);
                        printer.PageCount = pageCount;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo obtener el contador de páginas para {PrinterName}", printer.Name);
                    }

                    // Obtener niveles de tinta/tóner
                    try
                    {
                        var inkLevel = await _snmpService.GetInkLevelAsync(printer.Name, printer.IpAddress, printer.Model, printer.SnmpPort ?? 161);
                        // Nota: Este método devuelve un solo nivel, necesitaríamos llamarlo múltiples veces para diferentes colores
                        // Por ahora, asumimos que es el nivel de tóner negro
                        printer.BlackTonerLevel = inkLevel;
                        
                        // Verificar alerta de tóner bajo
                        printer.LowTonerWarning = inkLevel < 20;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo obtener el nivel de tinta para {PrinterName}", printer.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar impresora de red {PrinterIp}", printer.IpAddress);
                printer.Status = "Error";
                printer.IsOnline = false;
                printer.LastError = ex.Message;
            }
        }

    }
}
