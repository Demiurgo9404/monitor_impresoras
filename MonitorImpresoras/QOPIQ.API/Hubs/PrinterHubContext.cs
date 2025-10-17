using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using QOPIQ.API.Hubs;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Interfaces;

namespace QOPIQ.API.Hubs
{
    public class PrinterHubContext : IPrinterHubContext
    {
        private readonly IHubContext<PrinterHub, IPrinterHub> _hubContext;

        public PrinterHubContext(IHubContext<PrinterHub, IPrinterHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task SendPrinterUpdate(string printerId, string status)
        {
            return _hubContext.Clients.All.ReceivePrinterUpdate(printerId, status);
        }

        public Task SendPrinterAlert(string printerId, string alertMessage)
        {
            return _hubContext.Clients.All.ReceivePrinterAlert(printerId, alertMessage);
        }
    }
}
