using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Crea un nuevo tenant
        /// </summary>
        /// <param name="request">Datos del tenant</param>
        /// <returns>Tenant creado</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequestDTO request)
        {
            try
            {
                var tenant = await _tenantService.CreateTenantAsync(request);
                return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información del tenant actual
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Información del tenant</returns>
        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetTenant(int tenantId)
        {
            try
            {
                var tenant = await _tenantService.GetTenantAsync(tenantId);
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información del tenant por nombre
        /// </summary>
        /// <param name="tenantName">Nombre del tenant</param>
        /// <returns>Información del tenant</returns>
        [HttpGet("by-name/{tenantName}")]
        public async Task<IActionResult> GetTenantByName(string tenantName)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByNameAsync(tenantName);
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información del tenant del usuario actual
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Información del tenant</returns>
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetTenantByUserId(string userId)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByUserIdAsync(userId);
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza configuración del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="settings">Nuevas configuraciones</param>
        /// <returns>Tenant actualizado</returns>
        [HttpPut("{tenantId}/settings")]
        public async Task<IActionResult> UpdateTenantSettings(int tenantId, [FromBody] TenantSettingsDTO settings)
        {
            try
            {
                var tenant = await _tenantService.UpdateTenantAsync(tenantId, settings);
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Activa o desactiva un tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="isActive">Estado deseado</param>
        /// <returns>Tenant actualizado</returns>
        [HttpPatch("{tenantId}/status")]
        public async Task<IActionResult> ToggleTenantStatus(int tenantId, [FromQuery] bool isActive)
        {
            try
            {
                var tenant = await _tenantService.ToggleTenantStatusAsync(tenantId, isActive);
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Estadísticas del tenant</returns>
        [HttpGet("{tenantId}/statistics")]
        public async Task<IActionResult> GetTenantStatistics(int tenantId)
        {
            try
            {
                var statistics = await _tenantService.GetTenantStatisticsAsync(tenantId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene dashboard del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Dashboard completo</returns>
        [HttpGet("{tenantId}/dashboard")]
        public async Task<IActionResult> GetTenantDashboard(int tenantId)
        {
            try
            {
                var dashboard = await _tenantService.GetTenantDashboardAsync(tenantId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica límites del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="action">Acción a verificar</param>
        /// <returns>Resultado de la verificación</returns>
        [HttpGet("{tenantId}/limits")]
        public async Task<IActionResult> CheckLimits(int tenantId, [FromQuery] string action = "default")
        {
            try
            {
                var limits = await _tenantService.CheckLimitsAsync(tenantId, action);
                return Ok(limits);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los tenants (para superadmin)
        /// </summary>
        /// <param name="includeInactive">Incluir tenants inactivos</param>
        /// <returns>Lista de tenants</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTenants([FromQuery] bool includeInactive = false)
        {
            try
            {
                var tenants = await _tenantService.GetAllTenantsAsync(includeInactive);
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Busca tenants por criterios
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <param name="status">Estado del tenant</param>
        /// <param name="planType">Tipo de plan</param>
        /// <returns>Tenants encontrados</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchTenants(
            [FromQuery] string searchTerm,
            [FromQuery] string status,
            [FromQuery] string planType)
        {
            try
            {
                var tenants = await _tenantService.SearchTenantsAsync(searchTerm, status, planType);
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene configuración del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Configuración del tenant</returns>
        [HttpGet("{tenantId}/settings")]
        public async Task<IActionResult> GetTenantSettings(int tenantId)
        {
            try
            {
                var settings = await _tenantService.GetTenantSettingsAsync(tenantId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene actividades recientes del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="limit">Número de actividades</param>
        /// <returns>Actividades recientes</returns>
        [HttpGet("{tenantId}/activities")]
        public async Task<IActionResult> GetRecentActivities(int tenantId, [FromQuery] int limit = 10)
        {
            try
            {
                var activities = await _tenantService.GetRecentActivitiesAsync(tenantId, limit);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
