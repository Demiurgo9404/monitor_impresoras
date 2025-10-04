using Microsoft.Extensions.Diagnostics.HealthChecks;
using MonitorImpresoras.Application.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorImpresoras.Infrastructure.HealthChecks
{
    public class PrinterHealthCheck : IHealthCheck
    {
        private readonly IPrinterMonitoringService _printerService;

        public PrinterHealthCheck(IPrinterMonitoringService printerService)
        {
            _printerService = printerService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var printers = await _printerService.GetAllPrintersAsync();
                if (printers == null || !printers.Any())
                {
                    return HealthCheckResult.Degraded("No se encontraron impresoras configuradas");
                }

                return HealthCheckResult.Healthy("Todas las impresoras están operativas");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Error al verificar el estado de las impresoras", ex);
            }
        }
    }
}
