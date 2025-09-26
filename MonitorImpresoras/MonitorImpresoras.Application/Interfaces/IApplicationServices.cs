using System.Collections.Generic;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IPrinterService
    {
        Task<IEnumerable<PrinterListDTO>> GetAllPrintersAsync();
        Task<PrinterDTO> GetPrinterByIdAsync(Guid id);
        Task<PrinterDTO> AddPrinterAsync(CreatePrinterDTO printerDto);
        Task UpdatePrinterAsync(Guid id, UpdatePrinterDTO printerDto);
        Task DeletePrinterAsync(Guid id);
        Task<IEnumerable<PrinterListDTO>> GetPrintersByLocationAsync(string location);
        Task<PrinterDTO> GetPrinterWithConsumablesAsync(Guid id);
    }

    public interface IPrintJobService
    {
        Task<PrintJobDTO> GetPrintJobByIdAsync(Guid id);
        Task<IEnumerable<PrintJobDTO>> GetPrintJobsByFilterAsync(PrintJobFilterDTO filter);
        Task<PrintJobDTO> CreatePrintJobAsync(CreatePrintJobDTO printJobDto, string userId);
        Task<PrintJobSummaryDTO> GetPrintJobSummaryAsync(PrintJobFilterDTO filter);
        Task<bool> CancelPrintJobAsync(Guid id, string userId);
    }

    public interface IConsumableService
    {
        Task<ConsumableDTO> GetConsumableByIdAsync(Guid id);
        Task<IEnumerable<ConsumableDTO>> GetConsumablesByFilterAsync(ConsumableFilterDTO filter);
        Task<IEnumerable<ConsumableDTO>> GetConsumablesByPrinterIdAsync(Guid printerId);
        Task<ConsumableDTO> UpdateConsumableLevelAsync(UpdateConsumableLevelDTO updateDto);
        Task<ConsumableStatsDTO> GetConsumableStatsAsync();
        Task CheckAndCreateLowConsumableAlertsAsync();
    }

    public interface IAlertEngineService
    {
        Task<int> ProcessAllAlertRulesAsync();
    }
}
