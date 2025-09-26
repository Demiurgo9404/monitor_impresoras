using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Hubs
{
    [Authorize]
    public class PrinterHub : Hub
    {
        private readonly ILogger<PrinterHub> _logger;
        private readonly IPrinterService _printerService;
        private readonly IAlertService _alertService;
        private readonly IConsumableService _consumableService;

        public PrinterHub(
            ILogger<PrinterHub> logger,
            IPrinterService printerService,
            IAlertService alertService,
            IConsumableService consumableService)
        {
            _logger = logger;
            _printerService = printerService;
            _alertService = alertService;
            _consumableService = consumableService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("uid")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation("User {UserId} connected to SignalR", userId);
            }

            if (Context.User.IsInRole("Admin") || Context.User.IsInRole("Technician"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "technicians");
                _logger.LogInformation("Technician/Admin connected to SignalR");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("uid")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation("User {UserId} disconnected from SignalR", userId);
            }

            if (Context.User.IsInRole("Admin") || Context.User.IsInRole("Technician"))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "technicians");
                _logger.LogInformation("Technician/Admin disconnected from SignalR");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Client can subscribe to printer status updates
        public async Task SubscribeToPrinter(int printerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"printer-{printerId}");
            _logger.LogInformation("Client subscribed to printer {PrinterId} updates", printerId);

            // Send current printer status immediately
            var printer = await _printerService.GetPrinterByIdAsync(printerId);
            if (printer != null)
            {
                await Clients.Caller.SendAsync("PrinterStatusUpdate", printer);
            }
        }

        // Client can unsubscribe from printer status updates
        public async Task UnsubscribeFromPrinter(int printerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"printer-{printerId}");
            _logger.LogInformation("Client unsubscribed from printer {PrinterId} updates", printerId);
        }

        // Subscribe to all printer updates
        public async Task SubscribeToAllPrinters()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "all-printers");
            _logger.LogInformation("Client subscribed to all printer updates");
        }

        // Unsubscribe from all printer updates
        public async Task UnsubscribeFromAllPrinters()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all-printers");
            _logger.LogInformation("Client unsubscribed from all printer updates");
        }

        // Subscribe to alert updates
        public async Task SubscribeToAlerts()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "alerts");
            _logger.LogInformation("Client subscribed to alert updates");
        }

        // Unsubscribe from alert updates
        public async Task UnsubscribeFromAlerts()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "alerts");
            _logger.LogInformation("Client unsubscribed from alert updates");
        }

        // Subscribe to consumable updates
        public async Task SubscribeToConsumables()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "consumables");
            _logger.LogInformation("Client subscribed to consumable updates");
        }

        // Unsubscribe from consumable updates
        public async Task UnsubscribeFromConsumables()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "consumables");
            _logger.LogInformation("Client unsubscribed from consumable updates");
        }

        // Request current printer summary
        public async Task RequestPrinterSummary()
        {
            try
            {
                var printers = await _printerService.GetAllPrintersAsync();
                var summary = new
                {
                    TotalPrinters = printers.Count(),
                    OnlinePrinters = printers.Count(p => p.IsOnline),
                    OfflinePrinters = printers.Count(p => !p.IsOnline),
                    Printers = printers
                };

                await Clients.Caller.SendAsync("PrinterSummary", summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer summary for SignalR");
                await Clients.Caller.SendAsync("Error", "Error retrieving printer summary");
            }
        }

        // Request current alerts summary
        public async Task RequestAlertsSummary()
        {
            try
            {
                var filter = new AlertFilterDTO
                {
                    PageNumber = 1,
                    PageSize = 100
                };

                var alerts = await _alertService.GetAlertsByFilterAsync(filter);
                var summary = new
                {
                    TotalAlerts = alerts.Count(),
                    ActiveAlerts = alerts.Count(a => a.Status == "Active"),
                    AcknowledgedAlerts = alerts.Count(a => a.Status == "Acknowledged"),
                    ResolvedAlerts = alerts.Count(a => a.Status == "Resolved"),
                    RecentAlerts = alerts.Take(10)
                };

                await Clients.Caller.SendAsync("AlertsSummary", summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts summary for SignalR");
                await Clients.Caller.SendAsync("Error", "Error retrieving alerts summary");
            }
        }

        // Request current consumables summary
        public async Task RequestConsumablesSummary()
        {
            try
            {
                var stats = await _consumableService.GetConsumableStatsAsync();
                var filter = new ConsumableFilterDTO
                {
                    Status = "low",
                    PageNumber = 1,
                    PageSize = 50
                };

                var lowConsumables = await _consumableService.GetConsumablesByFilterAsync(filter);

                var summary = new
                {
                    Stats = stats,
                    LowConsumables = lowConsumables
                };

                await Clients.Caller.SendAsync("ConsumablesSummary", summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consumables summary for SignalR");
                await Clients.Caller.SendAsync("Error", "Error retrieving consumables summary");
            }
        }

        // Send test notification (for debugging)
        public async Task SendTestNotification()
        {
            await Clients.Caller.SendAsync("TestNotification", new
            {
                Message = "Test notification from SignalR",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
