using QOPIQ.Application.Interfaces;
using System.Net;

namespace QOPIQ.API.Middleware
{
    /// <summary>
    /// Middleware que resuelve y valida el tenant para cada petición
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver, ITenantAccessor tenantAccessor)
        {
            // Rutas que no requieren tenant (health checks, swagger, etc.)
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (IsExcludedPath(path))
            {
                await _next(context);
                return;
            }

            try
            {
                // Resolver tenant
                var tenantId = tenantResolver.GetCurrentTenantId();
                
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    _logger.LogWarning("Request to {Path} without tenant header", context.Request.Path);
                    await WriteErrorResponse(context, HttpStatusCode.Forbidden, "Tenant header is required");
                    return;
                }

                // Validar tenant
                var isValid = await tenantResolver.IsValidTenantAsync(tenantId);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid tenant {TenantId} for path {Path}", tenantId, context.Request.Path);
                    await WriteErrorResponse(context, HttpStatusCode.Forbidden, "Invalid or inactive tenant");
                    return;
                }

                // Obtener información del tenant
                var tenantInfo = await tenantResolver.GetCurrentTenantAsync();
                
                // Establecer tenant en el accessor
                tenantAccessor.SetTenant(tenantId, tenantInfo);

                // Agregar tenant al contexto de logs
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["TenantId"] = tenantId,
                    ["TenantName"] = tenantInfo?.Name ?? "Unknown"
                }))
                {
                    _logger.LogDebug("Request processed for tenant {TenantId}", tenantId);
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tenant middleware for path {Path}", context.Request.Path);
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

        private static bool IsExcludedPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var excludedPaths = new[]
            {
                "/health",
                "/swagger",
                "/api-docs",
                "/.well-known",
                "/favicon.ico",
                "/robots.txt"
            };

            return excludedPaths.Any(excluded => path.StartsWith(excluded));
        }

        private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = message,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }

    /// <summary>
    /// Extension method para registrar el middleware
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}

