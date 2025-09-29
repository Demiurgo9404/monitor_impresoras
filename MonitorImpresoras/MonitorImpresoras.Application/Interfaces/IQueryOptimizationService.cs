using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de optimización avanzada de consultas SQL
    /// </summary>
    public interface IQueryOptimizationService
    {
        /// <summary>
        /// Ejecuta análisis completo de optimización de consultas
        /// </summary>
        Task<QueryOptimizationReport> RunCompleteQueryOptimizationAnalysisAsync();

        /// <summary>
        /// Crea índices recomendados en PostgreSQL
        /// </summary>
        Task<IndexCreationResult> CreateRecommendedIndexesAsync();
    }
}
