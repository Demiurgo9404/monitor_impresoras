using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces
{
    public interface IPrinterHub
    {
        Task ReceivePrinterUpdate(string printerId, string status);
        Task ReceivePrinterAlert(string printerId, string alertMessage);
    }
}
