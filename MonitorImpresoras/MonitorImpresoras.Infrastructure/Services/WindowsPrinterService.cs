using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class WindowsPrinterService : IWindowsPrinterService, IDisposable
    {
        private readonly ILogger<WindowsPrinterService> _logger;
        private bool _disposed = false;

        public WindowsPrinterService(ILogger<WindowsPrinterService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<string>> GetInstalledPrintersAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var query = new ObjectQuery("SELECT Name FROM Win32_Printer");
                    using var searcher = new ManagementObjectSearcher(query);
                    var printers = searcher.Get()
                        .Cast<ManagementObject>()
                        .Select(p => p["Name"]?.ToString())
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    _logger.LogInformation("Se encontraron {Count} impresoras instaladas", printers.Count);
                    return printers!;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener la lista de impresoras instaladas");
                    return new List<string>();
                }
            });
        }

        public async Task<bool> IsPrinterOnlineAsync(string printerName)
        {
            if (string.IsNullOrEmpty(printerName))
                return false;

            return await Task.Run(() =>
            {
                try
                {
                    var query = new ObjectQuery($"SELECT * FROM Win32_Printer WHERE Name = '{printerName.Replace("\\", "\\\\")}'");
                    using var searcher = new ManagementObjectSearcher(query);
                    var printer = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

                    if (printer == null)
                    {
                        _logger.LogWarning("Impresora no encontrada: {PrinterName}", printerName);
                        return false;
                    }

                    var status = (PrinterStatus)Convert.ToInt32(printer["PrinterStatus"]);
                    var isOnline = !IsPrinterInErrorState(status) && 
                                 (bool)printer["WorkOffline"] == false &&
                                 (bool)printer["Network"] == false; // Solo para impresoras locales

                    _logger.LogDebug("Estado de la impresora {PrinterName}: {Status} (En línea: {IsOnline})", 
                        printerName, status, isOnline);

                    return isOnline;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar el estado de la impresora {PrinterName}", printerName);
                    return false;
                }
            });
        }

        public async Task<int> GetJobCountAsync(string printerName)
        {
            if (string.IsNullOrEmpty(printerName))
                return 0;

            return await Task.Run(() =>
            {
                try
                {
                    var query = new ObjectQuery($"SELECT * FROM Win32_PrintJob WHERE Name LIKE '%{printerName.Replace("\\", "\\\\")}%'");
                    using var searcher = new ManagementObjectSearcher(query);
                    return searcher.Get().Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener el conteo de trabajos para la impresora {PrinterName}", printerName);
                    return 0;
                }
            });
        }

        public async Task<Dictionary<string, object>> GetPrinterStatusAsync(string printerName)
        {
            var result = new Dictionary<string, object>
            {
                ["Name"] = printerName,
                ["Status"] = "Unknown",
                ["LastChecked"] = DateTime.UtcNow
            };

            if (string.IsNullOrEmpty(printerName))
            {
                result["Status"] = "Nombre de impresora no proporcionado";
                result["ErrorMessage"] = "Nombre de impresora no proporcionado";
                return result;
            }

            try
            {
                var query = new ObjectQuery($"SELECT * FROM Win32_Printer WHERE Name = '{printerName.Replace("\\", "\\\\")}'");
                using var searcher = new ManagementObjectSearcher(query);
                var printer = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

                if (printer == null)
                {
                    result["Status"] = "No encontrada";
                    result["LastError"] = $"Impresora no encontrada: {printerName}";
                    _logger.LogWarning("Impresora no encontrada: {PrinterName}", printerName);
                    return result;
                }

                var status = (PrinterStatus)Convert.ToInt32(printer["PrinterStatus"]);
                var isPaused = (bool)printer["Paused"];
                var isInError = IsPrinterInErrorState(status);
                var isOffline = (bool)printer["WorkOffline"];
                var isOnline = !isInError && !isOffline;
                var statusMessage = GetPrinterStatusMessage(status, isPaused, isOffline);
                var pagesPrinted = printer["PagesPrinted"] != null ? Convert.ToInt32(printer["PagesPrinted"]) : 0;
                var jobCount = await GetJobCountAsync(printerName);

                result["Status"] = statusMessage;
                result["PageCount"] = pagesPrinted;
                result["JobCount"] = jobCount;
                result["IsPaused"] = isPaused;
                result["IsInError"] = isInError;
                result["ErrorMessage"] = isInError ? statusMessage : string.Empty;
                result["IsDefault"] = (bool)printer["Default"];
                result["IsNetwork"] = (bool)printer["Network"];
                result["IsLocal"] = !(bool)printer["Network"];
                result["IsShared"] = (bool)printer["Shared"];
                result["ShareName"] = printer["ShareName"]?.ToString() ?? string.Empty;
                result["PortName"] = printer["PortName"]?.ToString() ?? string.Empty;
                result["DriverName"] = printer["DriverName"]?.ToString() ?? string.Empty;
                result["Location"] = printer["Location"]?.ToString() ?? string.Empty;
                result["Comment"] = printer["Comment"]?.ToString() ?? string.Empty;

                _logger.LogInformation("Estado de la impresora {PrinterName}: {Status} (En línea: {IsOnline}, Trabajos: {JobCount})",
                    printerName, statusMessage, isOnline, jobCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la impresora {PrinterName}", printerName);
                result["Status"] = "Error al obtener estado";
                result["ErrorMessage"] = ex.Message;
                result["IsInError"] = true;
            }

            return result;
        }

        private bool IsPrinterInErrorState(PrinterStatus status)
        {
            return status == PrinterStatus.Error ||
                   status == PrinterStatus.NoToner ||
                   status == PrinterStatus.OutOfMemory ||
                   status == PrinterStatus.PaperOut || // Reemplazado OutOfPaper por PaperOut
                   status == PrinterStatus.OutputBinFull ||
                   status == PrinterStatus.PaperJam ||
                   status == PrinterStatus.PaperProblem ||
                   status == PrinterStatus.UserIntervention ||
                   status == PrinterStatus.DoorOpen;
        }

        private string GetPrinterStatusMessage(PrinterStatus status, bool isPaused, bool isOffline)
        {
            if (isOffline) return "Desconectada";
            if (isPaused) return "En pausa";

            return status switch
            {
                PrinterStatus.Other => "Otro estado",
                PrinterStatus.Unknown => "Estado desconocido",
                PrinterStatus.Idle => "Inactiva",
                PrinterStatus.Printing => "Imprimiendo",
                PrinterStatus.Warmup => "Calentando",
                PrinterStatus.StoppedPrinting => "Detenida",
                PrinterStatus.Offline => "Desconectada",
                PrinterStatus.PaperOut => "Sin papel",
                PrinterStatus.OutputBinFull => "Bandeja de salida llena",
                PrinterStatus.NotAvailable => "No disponible",
                PrinterStatus.Waiting => "En espera",
                PrinterStatus.Processing => "Procesando",
                PrinterStatus.Initializing => "Inicializando",
                PrinterStatus.WarmingUp => "Calentando",
                PrinterStatus.TonerLow => "Tóner bajo",
                PrinterStatus.NoToner => "Sin tóner",
                PrinterStatus.PagePunt => "Error de página",
                PrinterStatus.UserIntervention => "Intervención del usuario requerida",
                PrinterStatus.OutOfMemory => "Sin memoria",
                PrinterStatus.DoorOpen => "Tapa abierta",
                PrinterStatus.ServerUnknown => "Servidor desconocido",
                PrinterStatus.PowerSave => "Modo de ahorro de energía",
                _ => status.ToString()
            };
        }

        public async Task<int> GetPrinterPageCountAsync(string printerName)
        {
            if (string.IsNullOrEmpty(printerName))
                return 0;

            return await Task.Run(() =>
            {
                try
                {
                    var query = new ObjectQuery($"SELECT * FROM Win32_PerfFormattedData_Spooler_PrintQueue WHERE Name = '{printerName.Replace("\\", "\\\\")}'");
                    using var searcher = new ManagementObjectSearcher(query);
                    var printer = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

                    if (printer?["TotalPagesPrinted"] != null)
                    {
                        return Convert.ToInt32(printer["TotalPagesPrinted"]);
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener el contador de páginas para la impresora {PrinterName}", printerName);
                    return 0;
                }
            });
        }

        public async Task<Dictionary<string, object>> CheckPrinterStatusAsync(string printerName)
        {
            var status = new Dictionary<string, object>();
            
            try
            {
                var printerStatus = await GetPrinterStatusAsync(printerName);
                var isOnline = await IsPrinterOnlineAsync(printerName);
                var pageCount = await GetPrinterPageCountAsync(printerName);
                
                var statusMessage = printerStatus.TryGetValue("Status", out var statusValue) ? statusValue?.ToString() : "Unknown";
                
                status["IsReady"] = isOnline && statusMessage == "Inactiva";
                status["StatusMessage"] = statusMessage;
                status["PagesPrinted"] = pageCount;
                status["IsOnline"] = isOnline;
                status["LastChecked"] = DateTime.UtcNow;
                
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de la impresora {PrinterName}", printerName);
                status["Error"] = ex.Message;
                status["IsReady"] = false;
                status["StatusMessage"] = "Error al verificar el estado";
                status["PagesPrinted"] = 0;
                status["IsOnline"] = false;
                status["LastChecked"] = DateTime.UtcNow;
                
                return status;
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
                    // Liberar recursos administrados si es necesario
                }
                _disposed = true;
            }
        }
    }

    // Enumeración de estados de impresora
    public enum PrinterStatus
    {
        Other = 1,
        Unknown = 2,
        Idle = 3,
        Printing = 4,
        Warmup = 5,
        StoppedPrinting = 6,
        Offline = 7,
        Paused = 8,
        Error = 9,
        Busy = 10,
        NotAvailable = 11,
        Waiting = 12,
        Processing = 13,
        Initializing = 14,
        WarmingUp = 15,
        TonerLow = 16,
        NoToner = 17,
        PagePunt = 18,
        UserIntervention = 19,
        OutOfMemory = 20,
        DoorOpen = 21,
        ServerUnknown = 22,
        PowerSave = 23,
        PaperJam = 24,
        PaperOut = 25,
        ManualFeed = 26,
        PaperProblem = 27,
        OutputBinFull = 28,
        NotConnected = 29,
        WaitingForPrinter = 30,
        ProcessingStopped = 31,
        InvalidPrintTicket = 32,
        SavingPrinter = 33,
        PrinterRequested = 34,
        PrinterReset = 35,
        Authorizing = 36,
        Initialization = 37,
        Shutdown = 38,
        Deleted = 39,
        Updating = 40,
        InputBinMissing = 41,
        NotAvailable2 = 42,
        WaitingForUpdate = 43,
        PowerSave2 = 44,
        Processing2 = 45,
        Offline2 = 46,
        Offline3 = 47,
        Stopped = 48,
        UnknownPrinter = 49,
        Ready = 50
    }
}
