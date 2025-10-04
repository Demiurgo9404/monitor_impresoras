using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestión de reportes programados
    /// </summary>
    public interface IScheduledReportService
    {
        /// <summary>
        /// Crea un nuevo reporte programado
        /// </summary>
        Task<ScheduledReportDto> CreateScheduledReportAsync(CreateScheduledReportDto createDto);

        /// <summary>
        /// Obtiene todos los reportes programados del tenant actual
        /// </summary>
        Task<List<ScheduledReportDto>> GetScheduledReportsAsync();

        /// <summary>
        /// Obtiene un reporte programado por ID
        /// </summary>
        Task<ScheduledReportDto?> GetScheduledReportByIdAsync(Guid id);

        /// <summary>
        /// Actualiza un reporte programado
        /// </summary>
        Task<ScheduledReportDto?> UpdateScheduledReportAsync(Guid id, CreateScheduledReportDto updateDto);

        /// <summary>
        /// Elimina un reporte programado
        /// </summary>
        Task<bool> DeleteScheduledReportAsync(Guid id);

        /// <summary>
        /// Activa o desactiva un reporte programado
        /// </summary>
        Task<bool> ToggleScheduledReportAsync(Guid id, bool isActive);

        /// <summary>
        /// Obtiene reportes programados que deben ejecutarse
        /// </summary>
        Task<List<ScheduledReportDto>> GetReportsToExecuteAsync();

        /// <summary>
        /// Ejecuta un reporte programado específico
        /// </summary>
        Task<bool> ExecuteScheduledReportAsync(Guid scheduledReportId);

        /// <summary>
        /// Actualiza la información de última ejecución
        /// </summary>
        Task UpdateLastRunAsync(Guid scheduledReportId, DateTime lastRun, DateTime? nextRun);
    }
}
