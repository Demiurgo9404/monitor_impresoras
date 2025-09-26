using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Middleware
{
    /// <summary>
    /// Middleware para verificar planes y límites de tenant
    /// </summary>
    public class TenantPlanMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantPlanMiddleware> _logger;
        private readonly ITenantService _tenantService;
        private readonly ISubscriptionService _subscriptionService;

        public TenantPlanMiddleware(
            RequestDelegate next,
            ILogger<TenantPlanMiddleware> logger,
            ITenantService tenantService,
            ISubscriptionService subscriptionService)
        {
            _next = next;
            _logger = logger;
            _tenantService = tenantService;
            _subscriptionService = subscriptionService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extraer tenant ID del header o del JWT token
            var tenantId = ExtractTenantId(context);
            if (tenantId.HasValue)
            {
                // Validar que el tenant existe y está activo
                var isValidTenant = await _tenantService.ValidateTenantAsync(tenantId.Value);
                if (!isValidTenant)
                {
                    _logger.LogWarning("Tenant {TenantId} no es válido o está inactivo", tenantId.Value);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Tenant no válido",
                        message = "El tenant no existe o está inactivo"
                    });
                    return;
                }

                // Verificar suscripción
                var hasActiveSubscription = await _subscriptionService.IsSubscriptionActiveAsync(tenantId.Value);
                if (!hasActiveSubscription)
                {
                    // Verificar si está en período de prueba
                    var isTrial = await _subscriptionService.IsInTrialPeriodAsync(tenantId.Value);
                    var trialDays = await _subscriptionService.GetTrialDaysRemainingAsync(tenantId.Value);

                    if (!isTrial || trialDays <= 0)
                    {
                        _logger.LogWarning("Tenant {TenantId} no tiene suscripción activa", tenantId.Value);
                        context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Suscripción requerida",
                            message = "Se requiere una suscripción activa para usar este servicio",
                            isTrial = false,
                            trialDaysRemaining = 0
                        });
                        return;
                    }

                    // Está en período de prueba
                    _logger.LogInformation("Tenant {TenantId} usando período de prueba, días restantes: {TrialDays}",
                        tenantId.Value, trialDays);

                    context.Items["IsTrial"] = true;
                    context.Items["TrialDaysRemaining"] = trialDays;
                }

                // Verificar límites para acciones específicas
                var endpoint = context.Request.Path.Value?.ToLower();
                var method = context.Request.Method.ToUpper();

                var limitsCheck = await _tenantService.CheckLimitsAsync(tenantId.Value, GetActionFromEndpoint(endpoint, method));
                if (!limitsCheck.CanAddPrinter && endpoint?.Contains("/printers") == true && method == "POST")
                {
                    context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Límite alcanzado",
                        message = "Has alcanzado el límite de impresoras para tu plan",
                        limits = limitsCheck
                    });
                    return;
                }

                if (!limitsCheck.CanAddUser && endpoint?.Contains("/users") == true && method == "POST")
                {
                    context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Límite alcanzado",
                        message = "Has alcanzado el límite de usuarios para tu plan",
                        limits = limitsCheck
                    });
                    return;
                }

                if (!limitsCheck.HasFeatureEnabled && RequiresFeature(endpoint, method))
                {
                    context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Característica no disponible",
                        message = "Tu plan actual no incluye esta característica",
                        limits = limitsCheck
                    });
                    return;
                }

                // Agregar información del tenant al contexto
                context.Items["TenantId"] = tenantId.Value;
                context.Items["LimitsCheck"] = limitsCheck;

                _logger.LogDebug("Tenant {TenantId} validado correctamente", tenantId.Value);
            }
            else if (RequiresTenant(context.Request.Path.Value))
            {
                _logger.LogWarning("Endpoint requiere tenant pero no se proporcionó");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Tenant requerido",
                    message = "Este endpoint requiere un tenant válido"
                });
                return;
            }

            await _next(context);
        }

        private int? ExtractTenantId(HttpContext context)
        {
            // Intentar obtener tenant ID del header X-Tenant-ID
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader))
            {
                if (int.TryParse(tenantIdHeader, out var tenantId))
                {
                    return tenantId;
                }
            }

            // Intentar obtener del JWT token (si está implementado)
            // var user = context.User;
            // if (user?.Identity?.IsAuthenticated == true)
            // {
            //     var tenantIdClaim = user.Claims.FirstOrDefault(c => c.Type == "tenant_id");
            //     if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId))
            //     {
            //         return tenantId;
            //     }
            // }

            // Para endpoints de tenant management, extraer del path
            var path = context.Request.Path.Value;
            if (path?.StartsWith("/api/tenants/") == true)
            {
                var segments = path.Split('/');
                if (segments.Length >= 3 && int.TryParse(segments[3], out var tenantId))
                {
                    return tenantId;
                }
            }

            return null;
        }

        private string GetActionFromEndpoint(string endpoint, string method)
        {
            if (endpoint?.Contains("/printers") == true && method == "POST")
                return "add_printer";
            if (endpoint?.Contains("/users") == true && method == "POST")
                return "add_user";
            if (endpoint?.Contains("/policies") == true && method == "POST")
                return "add_policy";
            if (endpoint?.Contains("/cost/") == true)
                return "cost_calculation";
            if (endpoint?.Contains("/reports/scheduled") == true)
                return "scheduled_reports";
            if (endpoint?.Contains("/api/") == true)
                return "api_access";

            return "default";
        }

        private bool RequiresFeature(string endpoint, string method)
        {
            if (endpoint?.Contains("/cost/") == true)
                return true;
            if (endpoint?.Contains("/policies/") == true && (method == "POST" || method == "PUT"))
                return true;
            if (endpoint?.Contains("/reports/scheduled") == true)
                return true;
            if (endpoint?.Contains("/api/") == true)
                return true;

            return false;
        }

        private bool RequiresTenant(string path)
        {
            // Endpoints que requieren tenant
            var tenantRequiredPaths = new[]
            {
                "/api/printers",
                "/api/users",
                "/api/policies",
                "/api/cost",
                "/api/reports",
                "/api/alerts"
            };

            return tenantRequiredPaths.Any(requiredPath =>
                path?.StartsWith(requiredPath, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}
