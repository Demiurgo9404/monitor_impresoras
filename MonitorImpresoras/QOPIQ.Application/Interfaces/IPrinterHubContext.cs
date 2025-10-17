using System.Threading.Tasks;
using QOPIQ.Domain.Interfaces;

namespace QOPIQ.Application.Interfaces
{
    public interface IPrinterHubContext
    {
        Task SendPrinterUpdate(string printerId, string status);
        Task SendPrinterAlert(string printerId, string alertMessage);
    }
}
