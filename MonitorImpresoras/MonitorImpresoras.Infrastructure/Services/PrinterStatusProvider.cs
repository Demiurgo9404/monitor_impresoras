using System;
using System.Threading.Tasks;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Infrastructure.Services.SNMP;
using MonitorImpresoras.Infrastructure.Services.WMI;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class PrinterStatusProvider : IPrinterStatusProvider
    {
        private readonly ISnmpService _snmpService;
        private readonly IWindowsPrinterService _windowsPrinterService;

        public PrinterStatusProvider(ISnmpService snmpService, IWindowsPrinterService windowsPrinterService)
        {
            _snmpService = snmpService ?? throw new ArgumentNullException(nameof(snmpService));
            _windowsPrinterService = windowsPrinterService ?? throw new ArgumentNullException(nameof(windowsPrinterService));
        }

        public async Task<PrinterStatusDto> GetPrinterStatusAsync(PrinterDto printer)
        {
            if (printer == null)
                throw new ArgumentNullException(nameof(printer));

            var status = new PrinterStatusDto
            {
                Id = printer.Id,
                Name = printer.Name,
                IpAddress = printer.IpAddress,
                LastChecked = DateTime.UtcNow
            };

            try
            {
                // Intentar obtener el estado vía SNMP
                status.Status = await _snmpService.GetPrinterStatusAsync(
                    printer.IpAddress, 
                    printer.CommunityString ?? "public", 
                    printer.SnmpPort ?? 161);

                // Obtener niveles de tinta si es una impresora a color
                if (printer.IsColorPrinter)
                {
                    // Aquí deberías tener los OIDs correctos para cada impresora
                    // Esto es solo un ejemplo
                    status.CyanInkLevel = await _snmpService.GetInkLevelAsync(
                        printer.IpAddress, 
                        ".1.3.6.1.2.1.43.11.1.1.9.1.1", // OID de ejemplo para cian
                        printer.CommunityString ?? "public",
                        printer.SnmpPort ?? 161);

                    // Repetir para otros colores...
                }

                // Obtener contador de páginas
                status.PageCount = await _snmpService.GetPageCountAsync(
                    printer.IpAddress,
                    printer.CommunityString ?? "public",
                    printer.SnmpPort ?? 161);
            }
            catch (Exception ex)
            {
                // Si falla SNMP, intentar con WMI para impresoras locales
                if (printer.IsLocal)
                {
                    try
                    {
                        var localStatus = await _windowsPrinterService.GetPrinterStatusAsync(printer.Name);
                        if (localStatus.TryGetValue("Status", out var statusValue))
                        {
                            status.Status = statusValue?.ToString() ?? "Unknown";
                        }
                        if (localStatus.TryGetValue("PageCount", out var pageCountValue) && pageCountValue is int pageCount)
                        {
                            status.PageCount = pageCount;
                        }
                    }
                    catch
                    {
                        // Si ambos métodos fallan, marcar como error
                        status.Status = "Error";
                        status.ErrorMessage = ex.Message;
                    }
                }
                else
                {
                    status.Status = "Error";
                    status.ErrorMessage = ex.Message;
                }
            }

            return status;
        }
    }

    public interface IPrinterStatusProvider
    {
        Task<PrinterStatusDto> GetPrinterStatusAsync(PrinterDto printer);
    }

    public class PrinterStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
        public int? CyanInkLevel { get; set; }
        public int? MagentaInkLevel { get; set; }
        public int? YellowInkLevel { get; set; }
        public int? BlackInkLevel { get; set; }
        public int? PageCount { get; set; }
        public DateTime? LastChecked { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
