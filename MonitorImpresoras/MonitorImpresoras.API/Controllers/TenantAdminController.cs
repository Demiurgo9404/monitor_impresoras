using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    /// <summary>
    /// Controlador para panel administrativo de tenants
    /// </summary>
    [ApiController]
    [Route("api/admin/tenants")]
    public class TenantAdminController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantAdminController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Obtiene todos los tenants (para superadmin)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTenants()
        {
            try
            {
                var tenants = await _tenantService.GetAllTenantsAsync();
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información detallada de un tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetTenantById(int tenantId)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdAsync(tenantId);
                var usageStats = await _tenantService.GetTenantUsageStatsAsync(tenantId);

                var tenantInfo = new
                {
                    Tenant = tenant,
                    UsageStats = usageStats,
                    CanAddPrinter = await _tenantService.CanAddResourceAsync(tenantId, TenantResourceType.Printer),
                    CanAddUser = await _tenantService.CanAddResourceAsync(tenantId, TenantResourceType.User),
                    CanAddStorage = await _tenantService.CanAddResourceAsync(tenantId, TenantResourceType.Storage)
                };

                return Ok(tenantInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo tenant (para superadmin)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            try
            {
                var tenant = await _tenantService.CreateTenantAsync(request);
                return CreatedAtAction(nameof(GetTenantById), new { tenantId = tenant.Id }, tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un tenant
        /// </summary>
        [HttpPut("{tenantId}")]
        public async Task<IActionResult> UpdateTenant(int tenantId, [FromBody] UpdateTenantRequest request)
        {
            try
            {
                var tenant = await _tenantService.UpdateTenantAsync(tenantId, request);
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un tenant (borrado lógico)
        /// </summary>
        [HttpDelete("{tenantId}")]
        public async Task<IActionResult> DeleteTenant(int tenantId)
        {
            try
            {
                await _tenantService.DeleteTenantAsync(tenantId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de uso de un tenant
        /// </summary>
        [HttpGet("{tenantId}/stats")]
        public async Task<IActionResult> GetTenantUsageStats(int tenantId)
        {
            try
            {
                var stats = await _tenantService.GetTenantUsageStatsAsync(tenantId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un tenant puede agregar recursos
        /// </summary>
        [HttpGet("{tenantId}/can-add/{resourceType}")]
        public async Task<IActionResult> CanAddResource(int tenantId, TenantResourceType resourceType)
        {
            try
            {
                var canAdd = await _tenantService.CanAddResourceAsync(tenantId, resourceType);
                return Ok(new { CanAdd = canAdd });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
