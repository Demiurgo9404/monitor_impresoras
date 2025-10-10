using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces.MultiTenancy;

namespace QOPIQ.Infrastructure;

/// <summary>
/// Middleware para la resolución del tenant en el pipeline de la aplicación.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, ITenantAccessor tenantAccessor)
    {
        try
        {
            _logger.LogDebug("Procesando solicitud para la resolución del tenant...");
            
            // Obtener el tenant actual
            var tenantId = tenantAccessor.TenantId;
            
            if (!string.IsNullOrEmpty(tenantId))
            {
                _logger.LogInformation("Tenant actual: {TenantId}", tenantId);
            }
            else
            {
                _logger.LogWarning("No se pudo determinar el tenant para la solicitud");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el tenant");
            throw;
        }

        await _next(context);
    }
}

/// <summary>
/// Métodos de extensión para configurar el middleware de resolución de tenant.
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    /// <summary>
    /// Agrega el middleware de resolución de tenant al pipeline de la aplicación.
    /// </summary>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
