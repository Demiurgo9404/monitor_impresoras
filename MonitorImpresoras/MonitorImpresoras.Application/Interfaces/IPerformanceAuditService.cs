using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de auditoría y monitoreo de rendimiento
    /// </summary>
    public interface IPerformanceAuditService
    {
        /// <summary>
        /// Realiza auditoría completa de rendimiento del sistema
        /// </summary>
        Task<PerformanceAuditReport> PerformFullAuditAsync();

        /// <summary>
        /// Obtiene métricas de rendimiento de consultas SQL
        /// </summary>
        Task<IEnumerable<DatabaseQueryMetric>> GetDatabaseQueryMetricsAsync(int topCount = 20);

        /// <summary>
        /// Registra métrica de rendimiento para análisis continuo
        /// </summary>
        void RecordPerformanceMetric(string operation, TimeSpan duration, bool success, string? details = null);
    }
}
