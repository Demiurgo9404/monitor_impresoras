using Microsoft.AspNetCore.SignalR;
using QOPIQ.API.Hubs;
using QOPIQ.Application.Interfaces;

namespace QOPIQ.API.Hubs
{
    public class PrinterHubContext : IPrinterHubContext
    {
        private readonly IHubContext<PrinterHub> _hubContext;

        public PrinterHubContext(IHubContext<PrinterHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public IHubContext<PrinterHub> HubContext => _hubContext;
    }
}
