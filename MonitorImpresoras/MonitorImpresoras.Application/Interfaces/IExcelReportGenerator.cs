using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Generador de reportes en formato Excel
    /// </summary>
    public interface IExcelReportGenerator
    {
        /// <summary>
        /// Genera un reporte Excel completo con múltiples hojas
        /// </summary>
        Task<byte[]> GenerateExcelReportAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte Excel de resumen
        /// </summary>
        Task<byte[]> GenerateSummaryExcelAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte Excel detallado por impresora
        /// </summary>
        Task<byte[]> GeneratePrinterDetailsExcelAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte Excel de consumibles
        /// </summary>
        Task<byte[]> GenerateConsumablesExcelAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un reporte Excel de costos con gráficos
        /// </summary>
        Task<byte[]> GenerateCostAnalysisExcelAsync(ReportDataDto reportData);

        /// <summary>
        /// Genera un archivo CSV simple
        /// </summary>
        Task<byte[]> GenerateCsvReportAsync(ReportDataDto reportData);
    }
}
