namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de integración avanzada con Redis
    /// </summary>
    public interface IRedisIntegrationService
    {
        /// <summary>
        /// Configura Redis con opciones avanzadas para producción
        /// </summary>
        Task<RedisConfigurationResult> ConfigureRedisAsync();

        /// <summary>
        /// Ejecuta operaciones avanzadas de mantenimiento en Redis
        /// </summary>
        Task<RedisMaintenanceResult> ExecuteRedisMaintenanceAsync();

        /// <summary>
        /// Implementa estrategias avanzadas de invalidación de caché
        /// </summary>
        Task<CacheInvalidationResult> ExecuteAdvancedCacheInvalidationAsync(string pattern = "*");

        /// <summary>
        /// Proporciona integración avanzada con pub/sub de Redis para eventos de caché
        /// </summary>
        Task<RedisPubSubResult> SetupCacheEventSubscriptionsAsync();
    }
}
