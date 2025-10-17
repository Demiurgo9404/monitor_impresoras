using Microsoft.AspNetCore.SignalR;
using QOPIQ.Domain.Interfaces;
using System.Threading.Tasks;

namespace QOPIQ.API.Hubs
{
    public class PrinterHubClient : IPrinterHub
    {
        private readonly IClientProxy _clientProxy;

        public PrinterHubClient(IClientProxy clientProxy)
        {
            _clientProxy = clientProxy;
        }

        public Task ReceivePrinterUpdate(string printerId, string status)
        {
            return _clientProxy.SendAsync("ReceivePrinterUpdate", printerId, status);
        }

        public Task ReceivePrinterAlert(string printerId, string alertMessage)
        {
            return _clientProxy.SendAsync("ReceivePrinterAlert", printerId, alertMessage);
        }
    }
}
