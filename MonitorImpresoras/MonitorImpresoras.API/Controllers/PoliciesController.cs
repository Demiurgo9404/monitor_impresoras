using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly IPrintingPolicyService _policyService;

        public PoliciesController(IPrintingPolicyService policyService)
        {
            _policyService = policyService;
        }

        /// <summary>
        /// Evalúa si una impresión cumple con las políticas configuradas
        /// </summary>
        /// <param name="request">Detalles de la impresión a evaluar</param>
        /// <returns>Resultado de la evaluación con políticas aplicadas</returns>
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyPolicies([FromBody] PolicyEvaluationRequestDTO request)
        {
            try
            {
                var result = await _policyService.EvaluatePoliciesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las políticas activas
        /// </summary>
        /// <returns>Lista de políticas configuradas</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllPolicies()
        {
            try
            {
                var policies = await _policyService.GetAllPoliciesAsync();
                return Ok(policies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una política específica por ID
        /// </summary>
        /// <param name="id">ID de la política</param>
        /// <returns>Política encontrada</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicy(int id)
        {
            try
            {
                var policy = await _policyService.GetPolicyByIdAsync(id);
                return Ok(policy);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva política
        /// </summary>
        /// <param name="policy">Política a crear</param>
        /// <returns>Política creada</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyRequestDTO policy)
        {
            try
            {
                var createdPolicy = await _policyService.CreatePolicyAsync(policy);
                return CreatedAtAction(nameof(GetPolicy), new { id = createdPolicy.Id }, createdPolicy);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una política existente
        /// </summary>
        /// <param name="id">ID de la política</param>
        /// <param name="policy">Datos actualizados</param>
        /// <returns>Política actualizada</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, [FromBody] UpdatePolicyRequestDTO policy)
        {
            try
            {
                var updatedPolicy = await _policyService.UpdatePolicyAsync(id, policy);
                return Ok(updatedPolicy);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una política
        /// </summary>
        /// <param name="id">ID de la política</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            try
            {
                await _policyService.DeletePolicyAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Activa o desactiva una política
        /// </summary>
        /// <param name="id">ID de la política</param>
        /// <param name="request">Estado deseado</param>
        /// <returns>Política actualizada</returns>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> TogglePolicyStatus(int id, [FromBody] PolicyStatusRequestDTO request)
        {
            try
            {
                var policy = await _policyService.TogglePolicyStatusAsync(id, request.IsActive);
                return Ok(policy);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de aplicación de políticas
        /// </summary>
        /// <param name="startDate">Fecha de inicio</param>
        /// <param name="endDate">Fecha de fin</param>
        /// <returns>Estadísticas de políticas</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetPolicyStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var statistics = await _policyService.GetPolicyStatisticsAsync(startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Simula la aplicación de políticas sin ejecutar la impresión
        /// </summary>
        /// <param name="request">Detalles de la impresión</param>
        /// <returns>Resultado de simulación</returns>
        [HttpPost("simulate")]
        public async Task<IActionResult> SimulatePolicies([FromBody] PolicyEvaluationRequestDTO request)
        {
            try
            {
                var result = await _policyService.SimulatePolicyEvaluationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
