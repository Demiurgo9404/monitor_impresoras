using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de health checks
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Obtiene health check básico de la aplicación
        /// </summary>
        Task<HealthCheckDto> GetBasicHealthAsync();

        /// <summary>
        /// Obtiene health check extendido con detalles de componentes
        /// </summary>
        Task<ExtendedHealthCheckDto> GetExtendedHealthAsync();
    }
}
