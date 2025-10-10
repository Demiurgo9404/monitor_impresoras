using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces.MultiTenancy;

namespace QOPIQ.Infrastructure.Services.MultiTenancy;

/// <summary>
/// Implementación de ITenantResolver que resuelve el identificador del tenant a partir del contexto HTTP.
/// </summary>
public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantResolver> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="TenantResolver"/>
    /// </summary>
    public TenantResolver(
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantResolver> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string?> ResolveTenantIdentifierAsync(CancellationToken cancellationToken = default)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            _logger.LogWarning("No se pudo resolver el tenant: HttpContext es nulo");
            return null;
        }

        try
        {
            _logger.LogDebug("Iniciando resolución del tenant...");

            // 1. Intentar obtener el tenant del encabezado
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdFromHeader) && 
                !string.IsNullOrEmpty(tenantIdFromHeader))
            {
                _logger.LogDebug("Tenant encontrado en encabezado: {TenantId}", tenantIdFromHeader);
                return tenantIdFromHeader.ToString();
            }

            // 2. Intentar obtener el tenant de la ruta
            if (_httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("tenant", out var tenantIdFromRoute) && 
                tenantIdFromRoute is string tenantIdStr && 
                !string.IsNullOrEmpty(tenantIdStr))
            {
                _logger.LogDebug("Tenant encontrado en ruta: {TenantId}", tenantIdStr);
                return tenantIdStr;
            }

            // 3. Intentar obtener el tenant de la consulta
            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("tenant", out var tenantIdFromQuery) && 
                !string.IsNullOrEmpty(tenantIdFromQuery))
            {
                _logger.LogDebug("Tenant encontrado en consulta: {TenantId}", tenantIdFromQuery);
                return tenantIdFromQuery.ToString();
            }

            // 4. Intentar obtener el tenant de las cookies
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("TenantId", out var tenantIdFromCookie) && 
                !string.IsNullOrEmpty(tenantIdFromCookie))
            {
                _logger.LogDebug("Tenant encontrado en cookies: {TenantId}", tenantIdFromCookie);
                return tenantIdFromCookie;
            }

            _logger.LogWarning("No se pudo resolver el tenant: No se encontró en encabezados, ruta, consulta ni cookies");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar resolver el tenant");
            throw;
        }
    }
}
