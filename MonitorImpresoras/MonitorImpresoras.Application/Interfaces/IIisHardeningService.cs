using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de hardening de IIS
    /// </summary>
    public interface IIisHardeningService
    {
        /// <summary>
        /// Ejecuta procedimientos completos de hardening de IIS
        /// </summary>
        Task<IisHardeningResult> HardenIisAsync();

        /// <summary>
        /// Obtiene recomendaciones adicionales de hardening para IIS
        /// </summary>
        Task<IEnumerable<IisHardeningRecommendation>> GetHardeningRecommendationsAsync();
    }
}
