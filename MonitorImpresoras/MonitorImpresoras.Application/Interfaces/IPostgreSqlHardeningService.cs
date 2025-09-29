using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de hardening de PostgreSQL
    /// </summary>
    public interface IPostgreSqlHardeningService
    {
        /// <summary>
        /// Ejecuta procedimientos completos de hardening de PostgreSQL
        /// </summary>
        Task<PostgreSqlHardeningResult> HardenPostgreSqlAsync();

        /// <summary>
        /// Obtiene recomendaciones adicionales de hardening para PostgreSQL
        /// </summary>
        Task<IEnumerable<PostgreSqlHardeningRecommendation>> GetHardeningRecommendationsAsync();
    }
}
