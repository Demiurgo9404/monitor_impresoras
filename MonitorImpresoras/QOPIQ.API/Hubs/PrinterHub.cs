using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace QOPIQ.API.Hubs
{
    [Authorize]
    public class PrinterHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrinterStatus(string printerId, string status)
        {
            await Clients.All.SendAsync("ReceivePrinterStatus", printerId, status);
        }
    }
}
