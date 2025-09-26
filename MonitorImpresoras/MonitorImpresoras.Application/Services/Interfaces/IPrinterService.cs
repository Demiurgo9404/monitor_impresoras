namespace MonitorImpresoras.Application.Services.Interfaces
{
    public interface IPrinterService
    {
        Task<IEnumerable<string>> GetPrintersAsync();
        Task<string> GetPrinterStatusAsync(Guid printerId);
    }
}
