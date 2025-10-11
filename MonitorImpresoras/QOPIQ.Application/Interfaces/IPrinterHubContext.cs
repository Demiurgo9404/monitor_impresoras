using Microsoft.AspNetCore.SignalR;

namespace QOPIQ.Application.Interfaces
{
    public interface IPrinterHubContext
    {
        IHubContext<PrinterHub> HubContext { get; }
    }

    public class PrinterHub : Hub
    {
        public async Task SendPrinterStatus(string printerId, string status)
        {
            await Clients.All.SendAsync("ReceivePrinterStatus", printerId, status);
        }
    }
}
