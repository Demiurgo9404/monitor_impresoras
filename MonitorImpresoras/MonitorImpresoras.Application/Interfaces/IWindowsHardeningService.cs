using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de hardening de Windows Server
    /// </summary>
    public interface IWindowsHardeningService
    {
        /// <summary>
        /// Ejecuta procedimientos completos de hardening de Windows Server
        /// </summary>
        Task<WindowsHardeningResult> HardenWindowsServerAsync();

        /// <summary>
        /// Obtiene recomendaciones de hardening adicionales
        /// </summary>
        Task<IEnumerable<WindowsHardeningRecommendation>> GetHardeningRecommendationsAsync();
    }
}
