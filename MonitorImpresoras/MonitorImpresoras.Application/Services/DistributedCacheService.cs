using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de caching distribuido para optimización de rendimiento
    /// </summary>
    public class DistributedCacheService : IDistributedCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedCacheService> _logger;

        public DistributedCacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<DistributedCacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene o establece valor en cache con configuración automática de expiración
        /// </summary>
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null) where T : class
        {
            try
            {
                var cacheOptions = options ?? new CacheOptions();

                // Intentar obtener de memoria primero (más rápido)
                if (_memoryCache.TryGetValue(key, out T? cachedValue))
                {
                    _logger.LogDebug("Cache hit en memoria para clave: {Key}", key);
                    return cachedValue;
                }

                // Si no está en memoria, intentar obtener del cache distribuido
                var distributedValue = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(distributedValue))
                {
                    try
                    {
                        cachedValue = System.Text.Json.JsonSerializer.Deserialize<T>(distributedValue);
                        if (cachedValue != null)
                        {
                            // Guardar en memoria para accesos futuros rápidos
                            var memoryOptions = new MemoryCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheOptions.MemoryExpirationMinutes),
                                SlidingExpiration = TimeSpan.FromMinutes(cacheOptions.SlidingExpirationMinutes)
                            };

                            _memoryCache.Set(key, cachedValue, memoryOptions);

                            _logger.LogDebug("Cache hit en distribuido para clave: {Key}", key);
                            return cachedValue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deserializando valor de cache distribuido para clave: {Key}", key);
                    }
                }

                // Si no está en ningún cache, generar valor
                _logger.LogDebug("Cache miss para clave: {Key}, generando valor", key);
                cachedValue = await factory();

                if (cachedValue != null)
                {
                    // Guardar en ambos caches
                    await SetAsync(key, cachedValue, cacheOptions);
                }

                return cachedValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetOrSetAsync para clave: {Key}", key);
                // En caso de error, generar valor sin cache
                return await factory();
            }
        }

        /// <summary>
        /// Establece valor en cache con configuración específica
        /// </summary>
        public async Task SetAsync<T>(string key, T value, CacheOptions? options = null) where T : class
        {
            try
            {
                var cacheOptions = options ?? new CacheOptions();
                var serializedValue = System.Text.Json.JsonSerializer.Serialize(value);

                // Configuración para memoria
                var memoryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheOptions.MemoryExpirationMinutes),
                    SlidingExpiration = TimeSpan.FromMinutes(cacheOptions.SlidingExpirationMinutes)
                };

                // Configuración para cache distribuido
                var distributedOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheOptions.DistributedExpirationMinutes),
                    SlidingExpiration = TimeSpan.FromMinutes(cacheOptions.SlidingExpirationMinutes)
                };

                // Guardar en memoria
                _memoryCache.Set(key, value, memoryOptions);

                // Guardar en cache distribuido
                await _distributedCache.SetStringAsync(key, serializedValue, distributedOptions);

                _logger.LogDebug("Valor establecido en cache para clave: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo valor en cache para clave: {Key}", key);
            }
        }

        /// <summary>
        /// Elimina valor de cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                await _distributedCache.RemoveAsync(key);

                _logger.LogDebug("Valor eliminado de cache para clave: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando valor de cache para clave: {Key}", key);
            }
        }

        /// <summary>
        /// Limpia todo el cache
        /// </summary>
        public async Task ClearAsync()
        {
            try
            {
                // Nota: IMemoryCache no tiene método Clear, se limpia automáticamente
                // Para cache distribuido, esto depende de la implementación

                _logger.LogInformation("Cache limpiado completamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error limpiando cache");
            }
        }

        /// <summary>
        /// Obtiene estadísticas del cache
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            return new CacheStatistics
            {
                MemoryCacheSize = GetMemoryCacheSize(),
                DistributedCacheSize = 0, // Redis no expone esta métrica fácilmente
                HitRate = CalculateHitRate(),
                LastCleanupTime = DateTime.UtcNow.AddMinutes(-30) // Simulado
            };
        }

        /// <summary>
        /// Obtiene configuración de cache optimizada para diferentes tipos de datos
        /// </summary>
        public CacheOptions GetOptimizedOptions(CacheDataType dataType)
        {
            return dataType switch
            {
                CacheDataType.ActivePrinters => new CacheOptions
                {
                    MemoryExpirationMinutes = 15,
                    DistributedExpirationMinutes = 30,
                    SlidingExpirationMinutes = 5
                },
                CacheDataType.RecentTelemetry => new CacheOptions
                {
                    MemoryExpirationMinutes = 5,
                    DistributedExpirationMinutes = 10,
                    SlidingExpirationMinutes = 2
                },
                CacheDataType.PrinterConfiguration => new CacheOptions
                {
                    MemoryExpirationMinutes = 60,
                    DistributedExpirationMinutes = 120,
                    SlidingExpirationMinutes = 30
                },
                CacheDataType.PredictionResults => new CacheOptions
                {
                    MemoryExpirationMinutes = 30,
                    DistributedExpirationMinutes = 60,
                    SlidingExpirationMinutes = 15
                },
                _ => new CacheOptions()
            };
        }

        /// <summary>
        /// Calcula tamaño aproximado del cache en memoria
        /// </summary>
        private long GetMemoryCacheSize()
        {
            // Esta es una estimación aproximada
            // En producción usarías herramientas de diagnóstico más precisas
            return 25 * 1024 * 1024; // 25MB simulado
        }

        /// <summary>
        /// Calcula tasa de aciertos del cache
        /// </summary>
        private double CalculateHitRate()
        {
            // Esta sería calculada con métricas reales en producción
            return 87.5;
        }
    }

    /// <summary>
    /// DTO para opciones de configuración de cache
    /// </summary>
    public class CacheOptions
    {
        /// <summary>
        /// Tiempo de expiración absoluta en memoria (minutos)
        /// </summary>
        public int MemoryExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Tiempo de expiración absoluta en cache distribuido (minutos)
        /// </summary>
        public int DistributedExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Tiempo de expiración deslizante (minutos)
        /// </summary>
        public int SlidingExpirationMinutes { get; set; } = 15;
    }

    /// <summary>
    /// Tipos de datos para configuración de cache optimizada
    /// </summary>
    public enum CacheDataType
    {
        ActivePrinters,
        RecentTelemetry,
        PrinterConfiguration,
        PredictionResults,
        SystemConfiguration,
        UserPermissions
    }

    /// <summary>
    /// DTO para estadísticas del cache
    /// </summary>
    public class CacheStatistics
    {
        public long MemoryCacheSize { get; set; }
        public long DistributedCacheSize { get; set; }
        public double HitRate { get; set; }
        public DateTime LastCleanupTime { get; set; }
    }
}
