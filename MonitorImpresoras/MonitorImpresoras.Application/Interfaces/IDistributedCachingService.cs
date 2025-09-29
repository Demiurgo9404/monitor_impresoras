namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de caching distribuido avanzado
    /// </summary>
    public interface IDistributedCachingService
    {
        /// <summary>
        /// Obtiene datos del caché distribuido con fallback a memoria
        /// </summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Establece datos en caché distribuido con TTL dinámico
        /// </summary>
        Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Elimina datos del caché distribuido y memoria
        /// </summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene múltiples elementos del caché en paralelo
        /// </summary>
        Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Establece múltiples elementos en caché en paralelo
        /// </summary>
        Task SetMultipleAsync<T>(Dictionary<string, T> keyValuePairs, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Obtiene estadísticas del caché distribuido
        /// </summary>
        Task<DistributedCacheStatistics> GetCacheStatisticsAsync();

        /// <summary>
        /// Limpia caché basado en patrones
        /// </summary>
        Task<int> ClearCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    }
}
