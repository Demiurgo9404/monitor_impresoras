namespace QOPIQ.Domain.Interfaces
{
    public interface ISnmpService
    {
        Task<string> GetPrinterStatusAsync(string ipAddress);
        Task<Dictionary<string, string>> GetPrinterInfoAsync(string ipAddress);
    }
}

