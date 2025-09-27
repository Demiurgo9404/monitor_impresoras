using System.Threading.Tasks;
using MonitorImpresoras.Domain.DTOs.Alerts;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface ISignalRNotificationService
    {
        Task NotifyPrinterStatusUpdate(object update);
        Task NotifyNewAlert(AlertDTO alert);
        Task NotifyAlertAcknowledged(int alertId, string acknowledgedBy, string userName);
        Task NotifyAlertResolved(int alertId, string resolvedBy, string userName, string resolutionNotes);
        Task NotifyLowConsumableAlert(object alert);
        Task NotifyMonitoringStatusChange(object status);
        Task SendTestNotification(string message);
    }
}
