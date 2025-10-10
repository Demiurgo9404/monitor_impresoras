using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces.MultiTenancy;

namespace QOPIQ.Infrastructure.Services.MultiTenancy;

/// <summary>
/// Middleware para la resolución del tenant en el pipeline de la aplicación.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="TenantResolutionMiddleware"/>
    /// </summary>
    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Procesa la solicitud HTTP para resolver el tenant actual.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, ITenantAccessor tenantAccessor)
    {
        _logger.LogDebug("Iniciando resolución del tenant para la solicitud...");

        try
        {
            // Resolver el tenant de forma asíncrona
            var tenantId = await tenantAccessor.GetTenantIdAsync(context.RequestAborted);
            
            // Almacenar el tenant en el contexto HTTP para su uso posterior
            context.Items["TenantId"] = tenantId;
            _logger.LogDebug("Tenant resuelto para la solicitud: {TenantId}", tenantId ?? "null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resolver el tenant para la solicitud");
            // Continuamos con el pipeline aunque falle la resolución del tenant
            // para permitir que otros middlewares manejen el error si es necesario
        }

        // Llamar al siguiente middleware en el pipeline
        await _next(context);
    }
}
