using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Infrastructure.Services
{
    public class WindowsPrinterService : IWindowsPrinterService
    {
        public async Task<List<Printer>> GetLocalPrintersAsync()
        {
            // Implementación básica - retorna lista vacía por ahora
            await Task.Delay(1);
            return new List<Printer>();
        }

        public async Task<bool> IsPrinterOnlineAsync(string printerName)
        {
            // Implementación básica - siempre retorna true por ahora
            await Task.Delay(1);
            return true;
        }
    }
}

