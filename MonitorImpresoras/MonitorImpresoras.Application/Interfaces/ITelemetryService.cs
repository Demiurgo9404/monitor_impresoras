using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface ITelemetryService
    {
        Task<PrinterTelemetry> GetLatestTelemetryAsync(int printerId);
        Task<bool> SaveTelemetryAsync(PrinterTelemetry telemetry);
        Task<List<PrinterTelemetry>> GetHistoricalDataAsync(int printerId, DateTime from, DateTime to);
    }
}
