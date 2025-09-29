using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de gestión de reportes
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Obtiene los reportes disponibles para un usuario según sus permisos
        /// </summary>
        Task<IEnumerable<ReportTemplateDto>> GetAvailableReportsAsync(string userId);

        /// <summary>
        /// Genera un reporte de forma asíncrona
        /// </summary>
        Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request, string userId, string ipAddress);

        /// <summary>
        /// Obtiene el historial de reportes de un usuario
        /// </summary>
        Task<IEnumerable<ReportHistoryDto>> GetReportHistoryAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Obtiene detalles de una ejecución específica de reporte
        /// </summary>
        Task<ReportResultDto?> GetReportExecutionAsync(int executionId, string userId);

        /// <summary>
        /// Descarga el contenido de un reporte generado
        /// </summary>
        Task<(byte[] Content, string ContentType, string FileName)> DownloadReportAsync(int executionId, string userId);

        /// <summary>
        /// Obtiene estadísticas de reportes del usuario
        /// </summary>
        Task<ReportStatisticsDto> GetReportStatisticsAsync(string userId);

        /// <summary>
        /// Cancela una ejecución de reporte en progreso
        /// </summary>
        Task<bool> CancelReportExecutionAsync(int executionId, string userId);
    }
}
