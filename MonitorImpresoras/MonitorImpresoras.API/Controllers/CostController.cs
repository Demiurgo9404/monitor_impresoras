using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CostController : ControllerBase
    {
        private readonly ICostCalculationService _costCalculationService;

        public CostController(ICostCalculationService costCalculationService)
        {
            _costCalculationService = costCalculationService;
        }

        /// <summary>
        /// Calcula el costo estimado de una impresión
        /// </summary>
        /// <param name="request">Parámetros de la impresión</param>
        /// <returns>Costo estimado con desglose</returns>
        [HttpPost("estimate")]
        public async Task<IActionResult> EstimateCost([FromBody] CostEstimateRequestDTO request)
        {
            try
            {
                var result = await _costCalculationService.CalculateCostAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las tarifas actuales del sistema
        /// </summary>
        /// <returns>Tarifas configuradas</returns>
        [HttpGet("rates")]
        public async Task<IActionResult> GetRates()
        {
            try
            {
                var rates = await _costCalculationService.GetCurrentRatesAsync();
                return Ok(rates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza las tarifas del sistema
        /// </summary>
        /// <param name="rates">Nuevas tarifas</param>
        /// <returns>Confirmación de actualización</returns>
        [HttpPut("rates")]
        public async Task<IActionResult> UpdateRates([FromBody] CostRatesDTO rates)
        {
            try
            {
                await _costCalculationService.UpdateRatesAsync(rates);
                return Ok(new { message = "Tarifas actualizadas correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Simula diferentes escenarios de costo
        /// </summary>
        /// <param name="scenarios">Lista de escenarios a simular</param>
        /// <returns>Resultados de simulación</returns>
        [HttpPost("simulate")]
        public async Task<IActionResult> SimulateCosts([FromBody] CostSimulationRequestDTO scenarios)
        {
            try
            {
                var results = await _costCalculationService.SimulateCostsAsync(scenarios);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
