using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Crea una nueva suscripción
        /// </summary>
        /// <param name="request">Datos de la suscripción</param>
        /// <returns>Suscripción creada</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequestDTO request)
        {
            try
            {
                var subscription = await _subscriptionService.CreateSubscriptionAsync(request);
                return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene suscripción por ID
        /// </summary>
        /// <param name="subscriptionId">ID de la suscripción</param>
        /// <returns>Suscripción encontrada</returns>
        [HttpGet("{subscriptionId}")]
        public async Task<IActionResult> GetSubscription(int subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionService.GetSubscriptionAsync(subscriptionId);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene suscripción activa del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Suscripción activa</returns>
        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetActiveSubscription(int tenantId)
        {
            try
            {
                var subscription = await _subscriptionService.GetActiveSubscriptionAsync(tenantId);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cambia el plan de una suscripción
        /// </summary>
        /// <param name="request">Datos del cambio de plan</param>
        /// <returns>Suscripción actualizada</returns>
        [HttpPut("change-plan")]
        public async Task<IActionResult> ChangePlan([FromBody] ChangePlanRequestDTO request)
        {
            try
            {
                var subscription = await _subscriptionService.ChangePlanAsync(request);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancela una suscripción
        /// </summary>
        /// <param name="request">Datos de cancelación</param>
        /// <returns>Suscripción cancelada</returns>
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionRequestDTO request)
        {
            try
            {
                var subscription = await _subscriptionService.CancelSubscriptionAsync(request);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reactiva una suscripción cancelada
        /// </summary>
        /// <param name="subscriptionId">ID de la suscripción</param>
        /// <param name="planId">Nuevo plan (opcional)</param>
        /// <returns>Suscripción reactivada</returns>
        [HttpPost("{subscriptionId}/reactivate")]
        public async Task<IActionResult> ReactivateSubscription(int subscriptionId, [FromQuery] int? planId = null)
        {
            try
            {
                var subscription = await _subscriptionService.ReactivateSubscriptionAsync(subscriptionId, planId);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Renueva una suscripción
        /// </summary>
        /// <param name="subscriptionId">ID de la suscripción</param>
        /// <returns>Suscripción renovada</returns>
        [HttpPost("{subscriptionId}/renew")]
        public async Task<IActionResult> RenewSubscription(int subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionService.RenewSubscriptionAsync(subscriptionId);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si una suscripción está activa
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>True si está activa</returns>
        [HttpGet("tenant/{tenantId}/is-active")]
        public async Task<IActionResult> IsSubscriptionActive(int tenantId)
        {
            try
            {
                var isActive = await _subscriptionService.IsSubscriptionActiveAsync(tenantId);
                return Ok(new { isActive });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene historial de suscripciones del tenant
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>Historial de suscripciones</returns>
        [HttpGet("tenant/{tenantId}/history")]
        public async Task<IActionResult> GetSubscriptionHistory(int tenantId)
        {
            try
            {
                var history = await _subscriptionService.GetSubscriptionHistoryAsync(tenantId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si el tenant está en período de prueba
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <returns>True si está en trial</returns>
        [HttpGet("tenant/{tenantId}/is-trial")]
        public async Task<IActionResult> IsInTrialPeriod(int tenantId)
        {
            try
            {
                var isTrial = await _subscriptionService.IsInTrialPeriodAsync(tenantId);
                var daysRemaining = await _subscriptionService.GetTrialDaysRemainingAsync(tenantId);
                return Ok(new { isTrial, daysRemaining });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si el tenant tiene una característica habilitada
        /// </summary>
        /// <param name="tenantId">ID del tenant</param>
        /// <param name="featureName">Nombre de la característica</param>
        /// <returns>True si la característica está habilitada</returns>
        [HttpGet("tenant/{tenantId}/features/{featureName}")]
        public async Task<IActionResult> HasFeatureEnabled(int tenantId, string featureName)
        {
            try
            {
                var hasFeature = await _subscriptionService.HasFeatureEnabledAsync(tenantId, featureName);
                return Ok(new { hasFeature });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
