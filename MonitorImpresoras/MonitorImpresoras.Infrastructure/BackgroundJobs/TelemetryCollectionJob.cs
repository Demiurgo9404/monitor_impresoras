using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.Infrastructure.BackgroundJobs
{
    public class TelemetryCollectionJob : BackgroundService
    {
        private readonly ILogger<TelemetryCollectionJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPrinterStatusService _printerStatusService;

        public TelemetryCollectionJob(
            ILogger<TelemetryCollectionJob> logger,
            IServiceProvider serviceProvider,
            IPrinterStatusService printerStatusService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _printerStatusService = printerStatusService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TelemetryCollectionJob iniciado");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
                    var printers = await GetActivePrintersAsync(scope);
                    
                    foreach (var printer in printers)
                    {
                        try
                        {
                            var telemetry = await _printerStatusService.GetPrinterStatusAsync(printer.Id);
                            await telemetryService.SaveTelemetryAsync(telemetry);
                            _logger.LogDebug("Telemetría guardada para impresora {PrinterId}", printer.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al recolectar telemetría para impresora {PrinterId}", printer.Id);
                        }
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo de recolección de telemetría");
                }
                
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task<List<Printer>> GetActivePrintersAsync(IServiceScope scope)
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            return await dbContext.Printers
                .Where(p => p.IsActive)
                .ToListAsync();
        }
    }
}
