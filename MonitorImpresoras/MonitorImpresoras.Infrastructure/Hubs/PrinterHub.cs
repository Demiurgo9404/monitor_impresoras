using Microsoft.AspNetCore.SignalR;

namespace MonitorImpresoras.Infrastructure.Hubs
{
    public class PrinterHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
