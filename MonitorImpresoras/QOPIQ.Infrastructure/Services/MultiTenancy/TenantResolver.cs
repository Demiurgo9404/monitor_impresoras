using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces.MultiTenancy;

namespace QOPIQ.Infrastructure;

/// <summary>
/// Implementación de ITenantResolver que resuelve el identificador del tenant a partir del contexto HTTP.
/// </summary>
public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantResolver> _logger;
    private string? _currentTenantId;

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
        // Si ya tenemos un tenant resuelto, lo devolvemos
        if (_currentTenantId != null)
        {
            return _currentTenantId;
        }

        // Usamos await para asegurar que el método sea verdaderamente asíncrono
        await Task.CompletedTask;
        
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
                _currentTenantId = tenantIdFromHeader.ToString();
                _logger.LogDebug("Tenant encontrado en encabezado: {TenantId}", _currentTenantId);
                return _currentTenantId;
            }

            // 2. Intentar obtener el tenant de la ruta
            if (_httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("tenant", out var tenantIdFromRoute) && 
                tenantIdFromRoute is string tenantIdStr && 
                !string.IsNullOrEmpty(tenantIdStr))
            {
                _currentTenantId = tenantIdStr;
                _logger.LogDebug("Tenant encontrado en ruta: {TenantId}", _currentTenantId);
                return _currentTenantId;
            }

            // 3. Intentar obtener el tenant de la consulta
            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("tenant", out var tenantIdFromQuery) && 
                !string.IsNullOrEmpty(tenantIdFromQuery))
            {
                _currentTenantId = tenantIdFromQuery.ToString();
                _logger.LogDebug("Tenant encontrado en consulta: {TenantId}", _currentTenantId);
                return _currentTenantId;
            }

            // 4. Intentar obtener el tenant de las cookies
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("TenantId", out var tenantIdFromCookie) && 
                !string.IsNullOrEmpty(tenantIdFromCookie))
            {
                _currentTenantId = tenantIdFromCookie;
                _logger.LogDebug("Tenant encontrado en cookies: {TenantId}", _currentTenantId);
                return _currentTenantId;
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

    /// <inheritdoc />
    public string? GetCurrentTenantId()
    {
        if (_currentTenantId != null)
        {
            return _currentTenantId;
        }

        // Intentar resolver el tenant de forma síncrona
        try
        {
            var task = Task.Run(async () => await ResolveTenantIdentifierAsync().ConfigureAwait(false));
            task.Wait();
            _currentTenantId = task.Result;
            return _currentTenantId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el ID del tenant de forma síncrona");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsValidTenantAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return false;
        }

        try
        {
            // En una implementación real, aquí validaríamos el tenant contra la base de datos
            // o algún otro almacén de datos. Por ahora, asumimos que cualquier tenant no vacío es válido.
            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el tenant {TenantId}", tenantId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<object?> GetCurrentTenantAsync()
    {
        var tenantId = await ResolveTenantIdentifierAsync();
        if (string.IsNullOrEmpty(tenantId))
        {
            return null;
        }

        try
        {
            // En una implementación real, aquí recuperaríamos la información completa del tenant
            // desde la base de datos. Por ahora, devolvemos un objeto anónimo con el ID.
            return new { Id = tenantId, Name = $"Tenant {tenantId}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la información del tenant {TenantId}", tenantId);
            return null;
        }
    }
}
