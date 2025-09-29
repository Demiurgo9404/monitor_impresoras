using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de reportes programados
    /// </summary>
    public interface IScheduledReportService
    {
        /// <summary>
        /// Obtiene los reportes programados de un usuario
        /// </summary>
        Task<IEnumerable<ScheduledReportDto>> GetUserScheduledReportsAsync(string userId);

        /// <summary>
        /// Obtiene un reporte programado específico
        /// </summary>
        Task<ScheduledReportDto?> GetScheduledReportByIdAsync(int scheduledReportId, string userId);

        /// <summary>
        /// Crea un nuevo reporte programado
        /// </summary>
        Task<ScheduledReportDto> CreateScheduledReportAsync(CreateScheduledReportDto request, string userId);

        /// <summary>
        /// Actualiza un reporte programado existente
        /// </summary>
        Task<bool> UpdateScheduledReportAsync(int scheduledReportId, UpdateScheduledReportDto request, string userId);

        /// <summary>
        /// Elimina un reporte programado
        /// </summary>
        Task<bool> DeleteScheduledReportAsync(int scheduledReportId, string userId);

        /// <summary>
        /// Obtiene reportes programados que están vencidos para ejecución
        /// </summary>
        Task<IEnumerable<Domain.Entities.ScheduledReport>> GetDueScheduledReportsAsync();

        /// <summary>
        /// Ejecuta un reporte programado
        /// </summary>
        Task ExecuteScheduledReportAsync(Domain.Entities.ScheduledReport scheduledReport, string executedBy = "system");

        /// <summary>
        /// Calcula la próxima ejecución basada en la expresión CRON
        /// </summary>
        DateTime CalculateNextExecution(string cronExpression);

        /// <summary>
        /// Envía un reporte programado por email
        /// </summary>
        Task SendScheduledReportByEmailAsync(Domain.Entities.ScheduledReport scheduledReport, ReportResultDto result);
    }
}
