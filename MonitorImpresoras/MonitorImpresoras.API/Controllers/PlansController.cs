using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Obtiene todos los planes disponibles
        /// </summary>
        /// <param name="includeInactive">Incluir planes inactivos</param>
        /// <returns>Lista de planes</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllPlans([FromQuery] bool includeInactive = false)
        {
            try
            {
                var plans = await _planService.GetAllPlansAsync(includeInactive);
                return Ok(plans);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene plan por ID
        /// </summary>
        /// <param name="planId">ID del plan</param>
        /// <returns>Plan encontrado</returns>
        [HttpGet("{planId}")]
        public async Task<IActionResult> GetPlan(int planId)
        {
            try
            {
                var plan = await _planService.GetPlanAsync(planId);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene plan por tipo
        /// </summary>
        /// <param name="planType">Tipo de plan</param>
        /// <returns>Plan encontrado</returns>
        [HttpGet("type/{planType}")]
        public async Task<IActionResult> GetPlanByType(PlanType planType)
        {
            try
            {
                var plan = await _planService.GetPlanByTypeAsync(planType);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo plan
        /// </summary>
        /// <param name="plan">Datos del plan</param>
        /// <returns>Plan creado</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] PlanDTO plan)
        {
            try
            {
                var createdPlan = await _planService.CreatePlanAsync(plan);
                return CreatedAtAction(nameof(GetPlan), new { id = createdPlan.Id }, createdPlan);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un plan existente
        /// </summary>
        /// <param name="planId">ID del plan</param>
        /// <param name="plan">Datos actualizados</param>
        /// <returns>Plan actualizado</returns>
        [HttpPut("{planId}")]
        public async Task<IActionResult> UpdatePlan(int planId, [FromBody] PlanDTO plan)
        {
            try
            {
                var updatedPlan = await _planService.UpdatePlanAsync(planId, plan);
                return Ok(updatedPlan);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un plan
        /// </summary>
        /// <param name="planId">ID del plan</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{planId}")]
        public async Task<IActionResult> DeletePlan(int planId)
        {
            try
            {
                await _planService.DeletePlanAsync(planId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Activa o desactiva un plan
        /// </summary>
        /// <param name="planId">ID del plan</param>
        /// <param name="isActive">Estado deseado</param>
        /// <returns>Plan actualizado</returns>
        [HttpPatch("{planId}/status")]
        public async Task<IActionResult> TogglePlanStatus(int planId, [FromQuery] bool isActive)
        {
            try
            {
                var plan = await _planService.TogglePlanStatusAsync(planId, isActive);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un plan tiene una característica habilitada
        /// </summary>
        /// <param name="planId">ID del plan</param>
        /// <param name="featureName">Nombre de la característica</param>
        /// <returns>True si la característica está habilitada</returns>
        [HttpGet("{planId}/features/{featureName}")]
        public async Task<IActionResult> HasFeatureEnabled(int planId, string featureName)
        {
            try
            {
                var hasFeature = await _planService.HasFeatureEnabledAsync(planId, featureName);
                return Ok(new { hasFeature });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
