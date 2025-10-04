using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio principal para gestión de reportes multi-tenant
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Genera un reporte en el formato especificado
        /// </summary>
        Task<ReportDto> GenerateReportAsync(GenerateReportDto generateDto);

        /// <summary>
        /// Obtiene lista de reportes con filtros y paginación
        /// </summary>
        Task<ReportListDto> GetReportsAsync(int pageNumber = 1, int pageSize = 10, ReportFiltersDto? filters = null);

        /// <summary>
        /// Obtiene un reporte por ID
        /// </summary>
        Task<ReportDto?> GetReportByIdAsync(Guid id);

        /// <summary>
        /// Descarga el archivo de un reporte
        /// </summary>
        Task<(byte[] fileData, string fileName, string contentType)> DownloadReportAsync(Guid reportId);

        /// <summary>
        /// Elimina un reporte
        /// </summary>
        Task<bool> DeleteReportAsync(Guid id);

        /// <summary>
        /// Reenvía un reporte por email
        /// </summary>
        Task<bool> ResendReportByEmailAsync(Guid reportId, string[] emailRecipients);

        /// <summary>
        /// Obtiene reportes de un proyecto específico
        /// </summary>
        Task<List<ReportDto>> GetProjectReportsAsync(Guid projectId);

        /// <summary>
        /// Verifica si el usuario tiene acceso al reporte
        /// </summary>
        Task<bool> HasAccessToReportAsync(Guid reportId, string userId);
    }
}
