using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MonitorImpresoras.API.Authorization
{
    /// <summary>
    /// Proveedor de políticas de autorización que evalúa claims dinámicos desde la base de datos
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Si la política ya existe, usar el proveedor por defecto
            if (!policyName.StartsWith("Permission:"))
            {
                return _fallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            // Para políticas de permisos dinámicos, crear una política que evalúe el claim específico
            var claimType = policyName.Replace("Permission:", "");

            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(claimType));

            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }
    }

    /// <summary>
    /// Requerimiento de autorización que verifica si el usuario tiene un claim específico
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string ClaimType { get; }

        public PermissionRequirement(string claimType)
        {
            ClaimType = claimType;
        }
    }

    /// <summary>
    /// Handler que evalúa si el usuario tiene el claim requerido
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var user = context.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Usuario no autenticado intentando acceder a recurso protegido por claim: {ClaimType}", requirement.ClaimType);
                context.Fail();
                return Task.CompletedTask;
            }

            // Verificar si el usuario tiene el claim requerido
            var hasClaim = user.HasClaim(c => c.Type == requirement.ClaimType);

            if (hasClaim)
            {
                _logger.LogDebug("Usuario {UserId} autorizado con claim: {ClaimType}",
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value, requirement.ClaimType);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Usuario {UserId} denegado acceso - falta claim: {ClaimType}",
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value, requirement.ClaimType);
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
