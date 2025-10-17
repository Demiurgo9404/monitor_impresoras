using Microsoft.AspNetCore.SignalR;

namespace QOPIQ.Domain.Interfaces
{
    public interface IPrinterHubClient
    {
        // Define the methods that the client should implement
        Task ReceivePrinterUpdate(string printerId, string status);
        Task ReceivePrinterAlert(string printerId, string alertMessage);
    }
}
