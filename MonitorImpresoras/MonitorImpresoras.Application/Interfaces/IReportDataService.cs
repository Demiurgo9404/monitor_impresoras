using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio para obtener y procesar datos para reportes
    /// </summary>
    public interface IReportDataService
    {
        /// <summary>
        /// Obtiene todos los datos necesarios para generar un reporte
        /// </summary>
        Task<ReportDataDto> GetReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Obtiene resumen de datos para reporte ejecutivo
        /// </summary>
        Task<ReportSummaryDto> GetReportSummaryAsync(Guid projectId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Obtiene datos detallados de impresoras para el período
        /// </summary>
        Task<List<PrinterReportDataDto>> GetPrinterReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Obtiene datos de consumibles para el período
        /// </summary>
        Task<List<ConsumableReportDataDto>> GetConsumableReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Obtiene análisis de costos para el período
        /// </summary>
        Task<ReportCostDataDto> GetCostReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Obtiene alertas para el período
        /// </summary>
        Task<List<AlertReportDataDto>> GetAlertReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd);
    }
}
