using Microsoft.AspNetCore.SignalR;

namespace QOPIQ.API.Hubs
{
    public class PrinterHub : Hub
    {
        public async Task NotifyStatusChange(string printerId, string status)
        {
            await Clients.All.SendAsync("ReceivePrinterStatus", printerId, status);
        }
    }
}
