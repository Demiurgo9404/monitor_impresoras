using System.Threading.Tasks;
using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Application.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportDTO> GetReportByIdAsync(Guid id);
        Task<IEnumerable<ReportDTO>> GetReportsByFilterAsync(ReportFilterDTO filter);
        Task<ReportDTO> CreateReportAsync(CreateReportDTO reportDto, string userId);
        Task<ReportDTO> GeneratePrinterUsageReportAsync(ReportFilterDTO filter);
        Task<ReportDTO> GenerateCostReportAsync(ReportFilterDTO filter);
        Task<ReportDTO> GenerateConsumableReportAsync(ReportFilterDTO filter);
        Task<byte[]> ExportReportToPdfAsync(Guid reportId);
        Task<byte[]> ExportReportToExcelAsync(Guid reportId);
        Task<bool> DeleteReportAsync(Guid id, string userId);
    }
}
