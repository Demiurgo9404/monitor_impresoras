using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QOPIQ.Application.Interfaces.MultiTenancy;

namespace QOPIQ.API.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver, ITenantAccessor tenantAccessor)
        {
            // Resolver el tenant actual
            var tenantId = await tenantResolver.ResolveTenantIdentifierAsync();
            
            // Establecer el tenant en el TenantAccessor
            tenantAccessor.TenantId = tenantId;
            
            // Continuar con el pipeline
            await _next(context);
        }
    }

    public static class TenantResolutionMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantResolutionMiddleware>();
        }
    }
}
