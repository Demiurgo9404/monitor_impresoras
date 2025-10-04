using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    /// <summary>
    /// Controlador para operaciones relacionadas con tenants
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ITenantResolver _tenantResolver;
        private readonly ILogger<TenantController> _logger;

        public TenantController(
            ITenantAccessor tenantAccessor,
            ITenantResolver tenantResolver,
            ILogger<TenantController> logger)
        {
            _tenantAccessor = tenantAccessor;
            _tenantResolver = tenantResolver;
            _logger = logger;
        }

        /// <summary>
        /// Health check específico para tenant
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult> GetTenantHealth()
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new
                    {
                        status = "Error",
                        message = "No tenant context available",
                        timestamp = DateTime.UtcNow
                    });
                }

                var tenantInfo = await _tenantResolver.GetCurrentTenantAsync();
                if (tenantInfo == null)
                {
                    return NotFound(new
                    {
                        status = "Error",
                        message = "Tenant not found",
                        tenantId,
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Tenant health check successful for {TenantId}", tenantId);

                return Ok(new
                {
                    status = "Healthy",
                    tenant = new
                    {
                        id = tenantInfo.TenantId,
                        name = tenantInfo.Name,
                        company = tenantInfo.CompanyName,
                        isActive = tenantInfo.IsActive,
                        expiresAt = tenantInfo.ExpiresAt
                    },
                    timestamp = DateTime.UtcNow,
                    message = "Tenant context is valid and active"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tenant health check");
                return StatusCode(500, new
                {
                    status = "Error",
                    message = "Internal server error during tenant health check",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Obtiene información del tenant actual
        /// </summary>
        [HttpGet("info")]
        public async Task<ActionResult> GetTenantInfo()
        {
            try
            {
                var tenantInfo = await _tenantResolver.GetCurrentTenantAsync();
                if (tenantInfo == null)
                {
                    return NotFound(new
                    {
                        message = "Tenant information not available",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(tenantInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant info");
                return StatusCode(500, new
                {
                    message = "Error retrieving tenant information",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Endpoint para testing - valida header de tenant
        /// </summary>
        [HttpGet("validate")]
        public ActionResult ValidateTenant()
        {
            var tenantId = _tenantAccessor.TenantId;
            var hasValidTenant = _tenantAccessor.HasTenant;

            return Ok(new
            {
                hasValidTenant,
                tenantId = tenantId ?? "none",
                timestamp = DateTime.UtcNow,
                headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            });
        }
    }
}
