namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio avanzado de health checks enterprise
    /// </summary>
    public interface IAdvancedHealthCheckService
    {
        /// <summary>
        /// Ejecuta health checks completos del sistema
        /// </summary>
        Task<ComprehensiveHealthReport> ExecuteCompleteHealthCheckAsync();
    }
}
