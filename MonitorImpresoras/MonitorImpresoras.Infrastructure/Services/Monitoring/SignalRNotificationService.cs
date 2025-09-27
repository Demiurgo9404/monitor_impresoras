using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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

        public async Task NotifyPrinterStatusUpdate(object update)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("PrinterStatusUpdate", update);
                _logger.LogInformation("SignalR notification sent for printer status update");
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
                await _hubContext.Clients.All.SendAsync("NewAlert", alert);
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

                await _hubContext.Clients.All.SendAsync("AlertAcknowledged", notification);
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

                await _hubContext.Clients.All.SendAsync("AlertResolved", notification);
                _logger.LogInformation("SignalR notification sent for alert {AlertId} resolved by {UserName}", alertId, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for alert resolved");
            }
        }

        public async Task NotifyLowConsumableAlert(object alert)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("LowConsumableAlert", alert);
                _logger.LogInformation("SignalR notification sent for low consumable alert");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification for low consumable alert");
            }
        }

        public async Task NotifyMonitoringStatusChange(object status)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("MonitoringStatusChange", status);
                _logger.LogInformation("SignalR notification sent for monitoring status change");
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

                await _hubContext.Clients.All.SendAsync("TestNotification", notification);
                _logger.LogInformation("Test notification sent via SignalR: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
            }
        }
    }
}
