using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces.Services;

namespace MonitorImpresoras.Infrastructure.Services.WMI
{
    public class WindowsPrinterService : IWindowsPrinterService, IDisposable
    {
        private readonly ILogger<WindowsPrinterService> _logger;
        private bool _disposed = false;
        private const string WMI_NAMESPACE = "\\\\.\\root\\cimv2";
        private const string WMI_QUERY_PRINTERS = "SELECT * FROM Win32_Printer";
        private const string WMI_QUERY_PRINTER_COUNTER = "SELECT * FROM Win32_PerfFormattedData_Spooler_PrintQueue WHERE Name = '{0}'";

        public WindowsPrinterService(ILogger<WindowsPrinterService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetLocalPrintersAsync()
        {
            return await Task.Run(() =>
            {
                var printers = new List<string>();
                try
                {
                    using var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, WMI_QUERY_PRINTERS);
                    foreach (ManagementObject printer in searcher.Get())
                    {
                        if (printer["Name"] != null)
                        {
                            printers.Add(printer["Name"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener impresoras locales");
                    throw new ApplicationException("Error al obtener la lista de impresoras locales", ex);
                }
                return printers;
            });
        }

        public async Task<int?> GetPrinterPageCountAsync(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                throw new ArgumentException("El nombre de la impresora no puede estar vacío", nameof(printerName));

            return await Task.Run(() =>
            {
                try
                {
                    // Primero, obtener el contador de páginas del spooler
                    var query = string.Format(WMI_QUERY_PRINTER_COUNTER, EscapeWmiQuery(printerName));
                    using var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, query);
                    
                    foreach (ManagementObject printer in searcher.Get())
                    {
                        if (printer["TotalJobsPrinted"] != null && 
                            int.TryParse(printer["TotalJobsPrinted"].ToString(), out int pageCount))
                        {
                            return (int?)pageCount;
                        }
                    }

                    // Si no se encuentra en el spooler, intentar con la clase Win32_Printer
                    using var printerSearcher = new ManagementObjectSearcher(WMI_NAMESPACE, WMI_QUERY_PRINTERS);
                    foreach (ManagementObject printer in printerSearcher.Get())
                    {
                        if (printer["Name"]?.ToString() == printerName && 
                            printer["TotalJobsPrinted"] != null &&
                            int.TryParse(printer["TotalJobsPrinted"].ToString(), out int printerPageCount))
                        {
                            return printerPageCount;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener el contador de páginas para la impresora {PrinterName}", printerName);
                    throw new ApplicationException($"Error al obtener el contador de páginas: {ex.Message}", ex);
                }

                return null;
            });
        }

        private string EscapeWmiQuery(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                .Replace("\\", "\\\\")  // Escape de backslashes
                .Replace("'", "''")         // Escape de comillas simples
                .Replace("\"", "\\\"")    // Escape de comillas dobles
                .Replace("\0", "");         // Eliminar caracteres nulos
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
                }
                _disposed = true;
            }
        }

        ~WindowsPrinterService()
        {
            Dispose(false);
        }
    }
}
