using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Infrastructure.Hubs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Services.Monitoring
{
    public class PrinterMonitoringBackgroundService : BackgroundService
    {
        private readonly ILogger<PrinterMonitoringBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<PrinterHub> _hubContext;
        private Timer _monitoringTimer;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(2); // Check every 2 minutes

        public PrinterMonitoringBackgroundService(
            ILogger<PrinterMonitoringBackgroundService> logger,
            IServiceProvider serviceProvider,
            IHubContext<PrinterHub> hubContext)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Printer Monitoring Background Service started");

            // Start the monitoring timer
            _monitoringTimer = new Timer(
                async state => await MonitorPrintersAsync(),
                null,
                TimeSpan.FromMinutes(1), // Start after 1 minute
                _monitoringInterval);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Printer Monitoring Background Service stopping");
        }

        private async Task MonitorPrintersAsync()
        {
            try
            {
                _logger.LogDebug("Starting printer monitoring cycle");

                using var scope = _serviceProvider.CreateScope();
                var printerService = scope.ServiceProvider.GetRequiredService<IPrinterService>();
                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                var consumableService = scope.ServiceProvider.GetRequiredService<IConsumableService>();
                var printerMonitoringService = scope.ServiceProvider.GetRequiredService<IPrinterMonitoringService>();

                // Get all printers
                var printers = await printerService.GetAllPrintersAsync();

                foreach (var printer in printers)
                {
                    try
                    {
                        // Check if printer is online/offline
                        var status = await printerMonitoringService.GetPrinterStatusAsync(printer);

                        // Notify clients about status change
                        if (status.IsOnline != printer.IsOnline)
                        {
                            await NotifyPrinterStatusChange(printer, status);
                        }

                        // Check for low consumables
                        await CheckConsumableLevels(printer);

                        _logger.LogDebug("Monitored printer {PrinterName} ({PrinterId})", printer.Name, printer.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error monitoring printer {PrinterName} ({PrinterId})", printer.Name, printer.Id);
                    }
                }

                // Check for low consumables across all printers
                await consumableService.CheckAndCreateLowConsumableAlertsAsync();

                _logger.LogDebug("Completed printer monitoring cycle");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in printer monitoring cycle");
            }
        }

        private async Task NotifyPrinterStatusChange(Printer printer, PrinterStatus status)
        {
            try
            {
                var statusMessage = status.IsOnline ? "online" : "offline";

                // Notify specific printer subscribers
                await _hubContext.Clients.Group($"printer-{printer.Id}")
                    .SendAsync("PrinterStatusUpdate", new
                    {
                        PrinterId = printer.Id,
                        PrinterName = printer.Name,
                        IsOnline = status.IsOnline,
                        Status = status.Status,
                        Timestamp = DateTime.UtcNow
                    });

                // Notify all printer subscribers
                await _hubContext.Clients.Group("all-printers")
                    .SendAsync("PrinterStatusUpdate", new
                    {
                        PrinterId = printer.Id,
                        PrinterName = printer.Name,
                        IsOnline = status.IsOnline,
                        Status = status.Status,
                        Timestamp = DateTime.UtcNow
                    });

                // Notify technicians if printer goes offline
                if (!status.IsOnline)
                {
                    await _hubContext.Clients.Group("technicians")
                        .SendAsync("PrinterOfflineAlert", new
                        {
                            PrinterId = printer.Id,
                            PrinterName = printer.Name,
                            IpAddress = printer.IpAddress,
                            Location = printer.Location,
                            Status = status.Status,
                            Timestamp = DateTime.UtcNow
                        });
                }

                _logger.LogInformation("Notified clients about printer {PrinterName} status change to {Status}",
                    printer.Name, statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying printer status change for {PrinterName}", printer.Name);
            }
        }

        private async Task CheckConsumableLevels(Printer printer)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var consumableService = scope.ServiceProvider.GetRequiredService<IConsumableService>();

                var filter = new ConsumableFilterDTO
                {
                    PrinterId = printer.Id,
                    NeedsReplacement = true,
                    PageNumber = 1,
                    PageSize = 10
                };

                var lowConsumables = await consumableService.GetConsumablesByFilterAsync(filter);

                if (lowConsumables.Any())
                {
                    // Notify consumable subscribers
                    await _hubContext.Clients.Group("consumables")
                        .SendAsync("LowConsumableAlert", new
                        {
                            PrinterId = printer.Id,
                            PrinterName = printer.Name,
                            LowConsumables = lowConsumables.Select(c => new
                            {
                                c.Id,
                                c.Name,
                                c.Type,
                                c.CurrentLevel,
                                c.MaxCapacity,
                                c.Status
                            }),
                            Timestamp = DateTime.UtcNow
                        });

                    // Notify technicians about critical consumables
                    var criticalConsumables = lowConsumables.Where(c => c.Status == "critical");
                    if (criticalConsumables.Any())
                    {
                        await _hubContext.Clients.Group("technicians")
                            .SendAsync("CriticalConsumableAlert", new
                            {
                                PrinterId = printer.Id,
                                PrinterName = printer.Name,
                                CriticalConsumables = criticalConsumables.Select(c => new
                                {
                                    c.Id,
                                    c.Name,
                                    c.Type,
                                    c.CurrentLevel,
                                    c.MaxCapacity,
                                    c.Status
                                }),
                                Timestamp = DateTime.UtcNow
                            });
                    }

                    _logger.LogInformation("Notified about low consumables for printer {PrinterName}", printer.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking consumable levels for printer {PrinterName}", printer.Name);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Printer Monitoring Background Service is stopping");

            // Dispose the timer
            _monitoringTimer?.Dispose();

            await base.StopAsync(stoppingToken);
        }
    }
}
