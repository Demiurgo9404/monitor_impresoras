#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces.MultiTenancy;

namespace QOPIQ.Infrastructure.Services.MultiTenancy;

/// <summary>
/// Implementación de ITenantAccessor que proporciona acceso al tenant actual.
/// </summary>
public class TenantAccessor : ITenantAccessor
{
    // Constantes
    private const string TenantIdHeader = "X-Tenant-Id";
    private const string TenantIdRouteKey = "tenant";
    private const string TenantIdQueryKey = "tenant";
    private const string TenantIdCookieKey = "TenantId";

    // Dependencias
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantResolver _tenantResolver;
    private readonly ILogger<TenantAccessor> _logger;

    // Estado
    private string? _tenantId;
    private bool _tenantResolved = false;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    
    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(TenantId))]
    public bool HasTenant => !string.IsNullOrEmpty(TenantId);
    
    /// <inheritdoc />
    public string? TenantId
    {
        get
        {
            if (_tenantResolved)
                return _tenantId;
                
            try
            {
                // Usar Task.Run para evitar deadlocks en contextos síncronos
                var task = Task.Run(async () => await GetTenantIdAsync().ConfigureAwait(false));
                task.Wait();
                return task.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el ID del tenant de forma síncrona");
                return null;
            }
        }
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="TenantAccessor"/>
    /// </summary>
    public TenantAccessor(
        IHttpContextAccessor httpContextAccessor,
        ITenantResolver tenantResolver,
        ILogger<TenantAccessor> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string? TenantId
    {
        get
        {
            // Si ya hemos resuelto el tenant, lo retornamos
            if (_tenantResolved)
            {
                return _tenantId;
            }

            // Si no, intentamos resolverlo de forma síncrona
            try
            {
                // Usamos ConfigureAwait(false) para evitar deadlocks
                var task = Task.Run(async () => await GetTenantIdAsync().ConfigureAwait(false));
                task.Wait(); // Bloqueante, pero solo en este contexto síncrono
                return task.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el ID del tenant de forma síncrona");
                return null;
            }
        }
    }

    /// <summary>
    /// Obtiene el ID del tenant de forma asíncrona.
    /// </summary>
    public async Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        // Si ya hemos resuelto el tenant, lo retornamos
        if (_tenantResolved)
        {
            return _tenantId;
        }

        try
        {
            // Usamos un semáforo para evitar múltiples resoluciones simultáneas
            await _semaphore.WaitAsync(cancellationToken);

            // Verificamos de nuevo por si otro hilo ya lo resolvió mientras esperábamos
            if (_tenantResolved)
            {
                return _tenantId;
            }

            // Resolvemos el tenant
            _tenantId = await _tenantResolver.ResolveTenantIdentifierAsync(cancellationToken);
            _tenantResolved = true;

            _logger.LogDebug("Tenant resuelto: {TenantId}", _tenantId ?? "null");
            return _tenantId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resolver el ID del tenant");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public void SetTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El ID del tenant no puede ser nulo o vacío.", nameof(tenantId));
        }

        _tenantId = tenantId;
        _tenantResolved = true;
        _logger.LogDebug("Tenant establecido manualmente: {TenantId}", tenantId);
    }
}
