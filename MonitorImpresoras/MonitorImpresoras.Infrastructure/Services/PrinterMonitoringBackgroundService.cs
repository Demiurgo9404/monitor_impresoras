using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Infrastructure.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class PrinterMonitoringBackgroundService : BackgroundService
    {
        private readonly ILogger<PrinterMonitoringBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PrinterMonitoringOptions _options;
        private readonly TimeSpan _monitoringInterval;

        public PrinterMonitoringBackgroundService(
            ILogger<PrinterMonitoringBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<PrinterMonitoringOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            
            // Configurar el intervalo de monitoreo desde las opciones
            _monitoringInterval = TimeSpan.FromMinutes(_options.MonitoringIntervalMinutes);
            
            _logger.LogInformation("Servicio de monitoreo configurado con un intervalo de {Minutes} minutos", 
                _options.MonitoringIntervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de monitoreo de impresoras iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var printerService = scope.ServiceProvider.GetRequiredService<IPrinterMonitoringService>();
                        var printers = await printerService.GetAllPrintersAsync();

                        _logger.LogInformation($"Monitoreando {printers.Count} impresoras...");

                        foreach (var printer in printers)
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;

                            try
                            {
                                await printerService.CheckPrinterStatusAsync(printer.Id);
                                _logger.LogDebug($"Estado de la impresora {printer.Name} verificado correctamente.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error al verificar el estado de la impresora {printer.Name} (ID: {printer.Id})");
                            }
                            
                            // Pequeña pausa entre verificaciones para no saturar
                            await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el servicio de monitoreo de impresoras");
                }

                // Esperar hasta la próxima verificación
                await Task.Delay(_monitoringInterval, stoppingToken);
            }

            _logger.LogInformation("Servicio de monitoreo de impresoras detenido.");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Deteniendo servicio de monitoreo de impresoras...");
            await base.StopAsync(stoppingToken);
        }
    }
}
