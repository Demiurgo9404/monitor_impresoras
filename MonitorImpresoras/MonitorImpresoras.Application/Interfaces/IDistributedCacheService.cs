using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de caching distribuido
    /// </summary>
    public interface IDistributedCacheService
    {
        /// <summary>
        /// Obtiene o establece valor en cache con configuración automática de expiración
        /// </summary>
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null) where T : class;

        /// <summary>
        /// Establece valor en cache con configuración específica
        /// </summary>
        Task SetAsync<T>(string key, T value, CacheOptions? options = null) where T : class;

        /// <summary>
        /// Elimina valor de cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Limpia todo el cache
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// Obtiene estadísticas del cache
        /// </summary>
        CacheStatistics GetStatistics();

        /// <summary>
        /// Obtiene configuración de cache optimizada para diferentes tipos de datos
        /// </summary>
        CacheOptions GetOptimizedOptions(CacheDataType dataType);
    }
}
