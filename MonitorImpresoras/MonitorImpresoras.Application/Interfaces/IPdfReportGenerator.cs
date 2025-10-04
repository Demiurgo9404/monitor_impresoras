using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Generador de reportes en formato PDF
    /// </summary>
    public interface IPdfReportGenerator
    {
        /// <summary>
        /// Genera un reporte PDF con los datos proporcionados
        /// </summary>
        Task<byte[]> GeneratePdfReportAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte PDF de resumen ejecutivo
        /// </summary>
        Task<byte[]> GenerateExecutiveSummaryPdfAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte PDF detallado por impresora
        /// </summary>
        Task<byte[]> GenerateDetailedPrinterReportAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte PDF de consumibles
        /// </summary>
        Task<byte[]> GenerateConsumableReportAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte PDF de costos
        /// </summary>
        Task<byte[]> GenerateCostReportAsync(ReportDataDto reportData);
    }
}
