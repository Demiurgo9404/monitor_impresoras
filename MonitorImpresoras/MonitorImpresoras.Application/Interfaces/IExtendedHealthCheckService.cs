using Microsoft.Extensions.Diagnostics.HealthChecks;
using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de health checks extendidos
    /// </summary>
    public interface IExtendedHealthCheckService
    {
        /// <summary>
        /// Ejecuta todos los health checks disponibles
        /// </summary>
        Task<ExtendedHealthReport> RunExtendedHealthChecksAsync();
    }
}
