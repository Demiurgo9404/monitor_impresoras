using System.Collections.Concurrent;
using System.Net;

namespace QOPIQ.API.Middleware
{
    /// <summary>
    /// Middleware de Rate Limiting para protección contra ataques DDoS
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IConfiguration _configuration;
        
        // Cache en memoria para contadores de requests por IP
        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
        private static readonly Timer _cleanupTimer = new(CleanupExpiredEntries, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var endpoint = context.Request.Path.Value?.ToLower() ?? "";

            // Configuración de límites por endpoint
            var (permitLimit, windowSeconds) = GetRateLimitsForEndpoint(endpoint);
            
            if (!IsRequestAllowed(clientIp, permitLimit, windowSeconds))
            {
                _logger.LogWarning("🚫 Rate limit excedido para IP {ClientIp} en endpoint {Endpoint}", clientIp, endpoint);
                
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("Retry-After", windowSeconds.ToString());
                
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Obtiene la IP real del cliente considerando proxies
        /// </summary>
        private static string GetClientIpAddress(HttpContext context)
        {
            // Verificar headers de proxy
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Obtiene los límites de rate limiting según el endpoint
        /// </summary>
        private (int permitLimit, int windowSeconds) GetRateLimitsForEndpoint(string endpoint)
        {
            // Endpoints de autenticación más restrictivos
            if (endpoint.Contains("/auth/") || endpoint.Contains("/login"))
            {
                return (10, 60); // 10 requests por minuto
            }

            // Endpoints de SNMP más restrictivos
            if (endpoint.Contains("/snmp/") || endpoint.Contains("/printers/"))
            {
                return (50, 60); // 50 requests por minuto
            }

            // Endpoints generales
            var defaultLimit = _configuration.GetValue("RateLimiting:PermitLimit", 100);
            var defaultWindow = _configuration.GetValue("RateLimiting:Window", 60);
            
            return (defaultLimit, defaultWindow);
        }

        /// <summary>
        /// Verifica si el request está permitido según los límites
        /// </summary>
        private static bool IsRequestAllowed(string clientIp, int permitLimit, int windowSeconds)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddSeconds(-windowSeconds);

            var clientInfo = _clients.AddOrUpdate(clientIp, 
                new ClientRequestInfo { RequestTimes = new List<DateTime> { now } },
                (key, existing) =>
                {
                    lock (existing)
                    {
                        // Limpiar requests antiguos
                        existing.RequestTimes.RemoveAll(time => time < windowStart);
                        
                        // Agregar request actual
                        existing.RequestTimes.Add(now);
                        
                        return existing;
                    }
                });

            lock (clientInfo)
            {
                return clientInfo.RequestTimes.Count <= permitLimit;
            }
        }

        /// <summary>
        /// Limpia entradas expiradas del cache
        /// </summary>
        private static void CleanupExpiredEntries(object? state)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-5);
            var keysToRemove = new List<string>();

            foreach (var kvp in _clients)
            {
                lock (kvp.Value)
                {
                    if (kvp.Value.RequestTimes.Count == 0 || kvp.Value.RequestTimes.Max() < cutoff)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _clients.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Información de requests por cliente
        /// </summary>
        private class ClientRequestInfo
        {
            public List<DateTime> RequestTimes { get; set; } = new();
        }
    }
}
