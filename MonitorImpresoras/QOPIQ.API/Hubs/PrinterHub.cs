using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QOPIQ.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace QOPIQ.API.Hubs
{
    [Authorize]
    public class PrinterHub : Hub<IPrinterHub>
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
            await Clients.All.ReceivePrinterUpdate(printerId, status);
        }

        public async Task SendPrinterAlert(string printerId, string alertMessage)
        {
            await Clients.All.ReceivePrinterAlert(printerId, alertMessage);
        }
    }
}
