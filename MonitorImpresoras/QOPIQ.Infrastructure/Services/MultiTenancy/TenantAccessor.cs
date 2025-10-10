#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces.MultiTenancy;

namespace QOPIQ.Infrastructure;

/// <summary>
/// Implementación de ITenantAccessor que proporciona acceso al tenant actual.
/// </summary>
public class TenantAccessor : ITenantAccessor
{
    // Estado
    private string? _tenantId;
    private bool _tenantResolved = false;
    private readonly object _lock = new object();
    
    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(TenantId))]
    public bool HasTenant => !string.IsNullOrEmpty(TenantId);
    
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
            return null;
        }
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="TenantAccessor"/>
    /// </summary>
    public TenantAccessor()
    {
    }

    /// <summary>
    /// Obtiene el ID del tenant de forma asíncrona.
    /// </summary>
    public Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tenantId);
    }

    /// <inheritdoc />
    public void SetTenant(string tenantId, object? tenantInfo = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El ID del tenant no puede ser nulo o vacío.", nameof(tenantId));
        }

        lock (_lock)
        {
            _tenantId = tenantId;
            _tenantResolved = true;
        }
    }
}
