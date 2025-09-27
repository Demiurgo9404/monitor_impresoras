using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface ISignalRNotificationService
    {
        Task NotifyNewAlert(AlertDTO alertDto);
        Task NotifyPrinterStatusUpdate(object status);
        Task NotifyAlertAcknowledged(int alertId, string acknowledgedBy, string userName);
        Task NotifyAlertResolved(int alertId, string resolvedBy, string userName, string resolutionNotes);
        Task NotifyLowConsumableAlert(object alert);
        Task NotifyMonitoringStatusChange(object status);
        Task SendTestNotification(string message);
    }
}
