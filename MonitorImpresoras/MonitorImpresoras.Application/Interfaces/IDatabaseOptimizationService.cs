using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de optimización de consultas y rendimiento de base de datos
    /// </summary>
    public interface IDatabaseOptimizationService
    {
        /// <summary>
        /// Ejecuta optimizaciones automáticas de consultas frecuentes
        /// </summary>
        Task<OptimizationResult> OptimizeQueriesAsync();

        /// <summary>
        /// Obtiene recomendaciones de optimización basadas en patrones de uso
        /// </summary>
        Task<IEnumerable<OptimizationRecommendation>> GetOptimizationRecommendationsAsync();

        /// <summary>
        /// Analiza consultas lentas y sugiere optimizaciones específicas
        /// </summary>
        Task<IEnumerable<SlowQueryAnalysis>> AnalyzeSlowQueriesAsync();
    }
}
