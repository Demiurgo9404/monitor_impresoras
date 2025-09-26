using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.API.Hubs;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services.Monitoring
{
    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<PrinterHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<PrinterHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyPrinterStatusUpdate(PrinterStatusUpdateDTO update)
        {
            try
            {
                var notification = new
                {
                    PrinterId = update.PrinterId,
                    PrinterName = update.PrinterName,
                    IsOnline = update.IsOnline,
                    Status = update.Status,
                    PreviousStatus = update.PreviousStatus,
                    Timestamp = DateTime.UtcNow
                };

                // Notify specific printer subscribers
                await _hubContext.Clients.Group($"printer-{update.PrinterId}")
                    .SendAsync("PrinterStatusUpdate", notification);

                // Notify all printer subscribers
                await _hubContext.Clients.Group("all-printers")
                    .SendAsync("PrinterStatusUpdate", notification);

                // Notify technicians if printer goes offline or comes back online
                if (!update.IsOnline || update.PreviousStatus == "offline")
                {
                    await _hubContext.Clients.Group("technicians")
                        .SendAsync(update.IsOnline ? "PrinterOnlineAlert" : "PrinterOfflineAlert", notification);
                }

                _logger.LogInformation("SignalR notification sent for printer {PrinterName} status change", update.PrinterName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for printer status update");
            }
        }

        public async Task NotifyNewAlert(AlertDTO alert)
        {
            try
            {
                var notification = new
                {
                    AlertId = alert.Id,
                    Type = alert.Type,
                    Title = alert.Title,
                    Message = alert.Message,
                    PrinterId = alert.PrinterId,
                    PrinterName = alert.PrinterName,
                    Severity = GetAlertSeverity(alert.Type),
                    Timestamp = DateTime.UtcNow
                };

                // Notify alert subscribers
                await _hubContext.Clients.Group("alerts")
                    .SendAsync("NewAlert", notification);

                // Notify technicians for all new alerts
                await _hubContext.Clients.Group("technicians")
                    .SendAsync("NewAlert", notification);

                // Notify specific printer subscribers if it's a printer-related alert
                if (alert.PrinterId.HasValue)
                {
                    await _hubContext.Clients.Group($"printer-{alert.PrinterId}")
                        .SendAsync("PrinterAlert", notification);
                }

                _logger.LogInformation("SignalR notification sent for new alert: {AlertTitle}", alert.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for new alert");
            }
        }

        public async Task NotifyAlertAcknowledged(int alertId, string acknowledgedBy, string userName)
        {
            try
            {
                var notification = new
                {
                    AlertId = alertId,
                    AcknowledgedBy = acknowledgedBy,
                    UserName = userName,
                    Timestamp = DateTime.UtcNow
                };

                // Notify alert subscribers
                await _hubContext.Clients.Group("alerts")
                    .SendAsync("AlertAcknowledged", notification);

                // Notify technicians
                await _hubContext.Clients.Group("technicians")
                    .SendAsync("AlertAcknowledged", notification);

                _logger.LogInformation("SignalR notification sent for alert {AlertId} acknowledged by {UserName}", alertId, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for alert acknowledged");
            }
        }

        public async Task NotifyAlertResolved(int alertId, string resolvedBy, string userName, string resolutionNotes)
        {
            try
            {
                var notification = new
                {
                    AlertId = alertId,
                    ResolvedBy = resolvedBy,
                    UserName = userName,
                    ResolutionNotes = resolutionNotes,
                    Timestamp = DateTime.UtcNow
                };

                // Notify alert subscribers
                await _hubContext.Clients.Group("alerts")
                    .SendAsync("AlertResolved", notification);

                // Notify technicians
                await _hubContext.Clients.Group("technicians")
                    .SendAsync("AlertResolved", notification);

                _logger.LogInformation("SignalR notification sent for alert {AlertId} resolved by {UserName}", alertId, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for alert resolved");
            }
        }

        public async Task NotifyLowConsumableAlert(ConsumableAlertDTO alert)
        {
            try
            {
                var notification = new
                {
                    PrinterId = alert.PrinterId,
                    PrinterName = alert.PrinterName,
                    ConsumableId = alert.ConsumableId,
                    ConsumableName = alert.ConsumableName,
                    Type = alert.Type,
                    CurrentLevel = alert.CurrentLevel,
                    MaxCapacity = alert.MaxCapacity,
                    Status = alert.Status,
                    Severity = alert.IsCritical ? "critical" : "warning",
                    Timestamp = DateTime.UtcNow
                };

                // Notify consumable subscribers
                await _hubContext.Clients.Group("consumables")
                    .SendAsync("LowConsumableAlert", notification);

                // Notify technicians
                await _hubContext.Clients.Group("technicians")
                    .SendAsync(alert.IsCritical ? "CriticalConsumableAlert" : "LowConsumableAlert", notification);

                _logger.LogInformation("SignalR notification sent for low consumable alert: {ConsumableName} in printer {PrinterName}",
                    alert.ConsumableName, alert.PrinterName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for low consumable alert");
            }
        }

        public async Task NotifyMonitoringStatusChange(MonitoringStatusDTO status)
        {
            try
            {
                var notification = new
                {
                    IsActive = status.IsActive,
                    Status = status.Status,
                    ActiveConnections = status.ActiveConnections,
                    MonitoredPrinters = status.MonitoredPrinters,
                    Timestamp = DateTime.UtcNow
                };

                // Notify technicians about monitoring status changes
                await _hubContext.Clients.Group("technicians")
                    .SendAsync("MonitoringStatusChange", notification);

                _logger.LogInformation("SignalR notification sent for monitoring status change: {Status}", status.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for monitoring status change");
            }
        }

        public async Task SendTestNotification(string message)
        {
            try
            {
                var notification = new
                {
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                // Send to all connected clients for testing
                await _hubContext.Clients.All.SendAsync("TestNotification", notification);

                _logger.LogInformation("Test notification sent via SignalR: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
            }
        }

        private string GetAlertSeverity(string alertType)
        {
            return alertType.ToLower() switch
            {
                "critical" or "error" or "offline" => "critical",
                "warning" or "low" => "warning",
                "info" or "maintenance" => "info",
                _ => "info"
            };
        }
    }

    public interface ISignalRNotificationService
    {
        Task NotifyPrinterStatusUpdate(PrinterStatusUpdateDTO update);
        Task NotifyNewAlert(AlertDTO alert);
        Task NotifyAlertAcknowledged(int alertId, string acknowledgedBy, string userName);
        Task NotifyAlertResolved(int alertId, string resolvedBy, string userName, string resolutionNotes);
        Task NotifyLowConsumableAlert(ConsumableAlertDTO alert);
        Task NotifyMonitoringStatusChange(MonitoringStatusDTO status);
        Task SendTestNotification(string message);
    }

    public class PrinterStatusUpdateDTO
    {
        public int PrinterId { get; set; }
        public string PrinterName { get; set; }
        public bool IsOnline { get; set; }
        public string Status { get; set; }
        public string PreviousStatus { get; set; }
    }

    public class ConsumableAlertDTO
    {
        public int PrinterId { get; set; }
        public string PrinterName { get; set; }
        public int ConsumableId { get; set; }
        public string ConsumableName { get; set; }
        public string Type { get; set; }
        public int? CurrentLevel { get; set; }
        public int? MaxCapacity { get; set; }
        public string Status { get; set; }
        public bool IsCritical { get; set; }
    }
}
