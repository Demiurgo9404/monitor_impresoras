using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Printing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class WindowsPrinterService : IWindowsPrinterService
    {
        public Task<IEnumerable<string>> GetInstalledPrintersAsync()
        {
            try
            {
                var printers = PrinterSettings.InstalledPrinters.Cast<string>();
                return Task.FromResult(printers);
            }
            catch (Exception ex)
            {
                // En un entorno real, deberías registrar este error
                Console.WriteLine($"Error al obtener impresoras instaladas: {ex.Message}");
                return Task.FromResult(Enumerable.Empty<string>());
            }
        }

        public Task<bool> IsPrinterOnlineAsync(string printerName)
        {
            try
            {
                var query = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
                foreach (ManagementObject printer in query.Get())
                {
                    if (printer["Name"].ToString().Equals(printerName, StringComparison.OrdinalIgnoreCase))
                    {
                        bool isOnline = (bool)printer["WorkOffline"] == false;
                        return Task.FromResult(isOnline);
                    }
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar el estado de la impresora {printerName}: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<int> GetJobCountAsync(string printerName)
        {
            try
            {
                using (var printServer = new LocalPrintServer())
                {
                    var queue = printServer.GetPrintQueue(printerName);
                    queue.Refresh();
                    return Task.FromResult(queue.NumberOfJobs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el número de trabajos para {printerName}: {ex.Message}");
                return Task.FromResult(-1);
            }
        }

        public Task<Dictionary<string, object>> GetPrinterStatusAsync(string printerName)
        {
            var status = new Dictionary<string, object>();
            
            try
            {
                var query = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
                foreach (ManagementObject printer in query.Get())
                {
                    if (printer["Name"].ToString().Equals(printerName, StringComparison.OrdinalIgnoreCase))
                    {
                        status["Name"] = printer["Name"]?.ToString() ?? "N/A";
                        status["Status"] = printer["Status"]?.ToString() ?? "N/A";
                        status["IsOnline"] = (bool)printer["WorkOffline"] == false;
                        status["IsDefault"] = (bool)printer["Default"];
                        status["IsNetworkPrinter"] = (bool)printer["Network"];
                        status["Location"] = printer["Location"]?.ToString() ?? "N/A";
                        status["PortName"] = printer["PortName"]?.ToString() ?? "N/A";
                        
                        // Obtener información de trabajos
                        try
                        {
                            using (var printServer = new LocalPrintServer())
                            {
                                var queue = printServer.GetPrintQueue(printerName);
                                queue.Refresh();
                                status["NumberOfJobs"] = queue.NumberOfJobs;
                                status["IsInError"] = queue.IsInError;
                                status["IsOutOfPaper"] = queue.IsOutOfPaper;
                                status["IsPaperJammed"] = queue.IsPaperJammed;
                                status["IsOutOfInkOrToner"] = queue.IsOutOfInkOrToner;
                            }
                        }
                        catch (Exception ex)
                        {
                            status["JobInfoError"] = $"Error al obtener información de trabajos: {ex.Message}";
                        }
                        
                        return Task.FromResult(status);
                    }
                }
                
                status["Error"] = $"No se encontró la impresora: {printerName}";
                return Task.FromResult(status);
            }
            catch (Exception ex)
            {
                status["Error"] = $"Error al obtener el estado de la impresora: {ex.Message}";
                return Task.FromResult(status);
            }
        }
    }
}
