using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para repositorio de reportes
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Obtiene templates de reportes disponibles para un usuario según sus claims
        /// </summary>
        Task<IEnumerable<ReportTemplate>> GetAvailableReportTemplatesAsync(string userId);

        /// <summary>
        /// Obtiene un template de reporte por ID
        /// </summary>
        Task<ReportTemplate?> GetReportTemplateByIdAsync(int templateId);

        /// <summary>
        /// Obtiene todas las ejecuciones de reportes de un usuario
        /// </summary>
        Task<IEnumerable<ReportExecution>> GetUserReportExecutionsAsync(string userId, int page, int pageSize);

        /// <summary>
        /// Obtiene una ejecución específica de reporte
        /// </summary>
        Task<ReportExecution?> GetReportExecutionByIdAsync(int executionId);

        /// <summary>
        /// Crea una nueva ejecución de reporte
        /// </summary>
        Task<ReportExecution> CreateReportExecutionAsync(ReportExecution execution);

        /// <summary>
        /// Actualiza una ejecución de reporte
        /// </summary>
        Task UpdateReportExecutionAsync(ReportExecution execution);

        /// <summary>
        /// Obtiene estadísticas de reportes para un usuario
        /// </summary>
        Task<ReportStatisticsDto> GetReportStatisticsAsync(string userId);

        /// <summary>
        /// Obtiene datos para generar reporte de impresoras
        /// </summary>
        Task<IEnumerable<object>> GetPrinterReportDataAsync(ReportFilterDto? filters = null);

        /// <summary>
        /// Obtiene datos para generar reporte de usuarios
        /// </summary>
        Task<IEnumerable<object>> GetUserReportDataAsync(ReportFilterDto? filters = null);

        /// <summary>
        /// Obtiene datos para generar reporte de auditoría
        /// </summary>
        Task<IEnumerable<object>> GetAuditReportDataAsync(ReportFilterDto? filters = null);

        /// <summary>
        /// Obtiene datos para generar reporte de permisos
        /// </summary>
        Task<IEnumerable<object>> GetPermissionsReportDataAsync(ReportFilterDto? filters = null);

        /// <summary>
        /// Elimina ejecuciones de reportes antiguas (cleanup)
        /// </summary>
        Task<int> CleanupOldReportExecutionsAsync(int retentionDays = 30);
    }
}
