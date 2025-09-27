using System.Threading;
using System.Threading.Tasks;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterMonitoringBackgroundService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
