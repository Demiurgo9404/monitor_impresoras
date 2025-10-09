using Microsoft.AspNetCore.Mvc;
using PrinterAgent.Core.Models;
using PrinterAgent.Core.Services;

namespace PrinterAgent.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentOrchestrator _orchestrator;
        private readonly ILogger<AgentController> _logger;

        public AgentController(IAgentOrchestrator orchestrator, ILogger<AgentController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el estado de salud del agente
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<AgentHealthStatus>> GetHealth()
        {
            try
            {
                var health = await _orchestrator.GetHealthStatusAsync();
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estado de salud");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene las métricas del agente
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<AgentMetrics>> GetMetrics()
        {
            try
            {
                var metrics = await _orchestrator.GetMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene la lista de impresoras monitoreadas
        /// </summary>
        [HttpGet("printers")]
        public async Task<ActionResult<List<PrinterInfo>>> GetPrinters()
        {
            try
            {
                var printers = await _orchestrator.GetMonitoredPrintersAsync();
                return Ok(printers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo impresoras");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Fuerza un escaneo de red
        /// </summary>
        [HttpPost("scan")]
        public async Task<ActionResult> ForceScan()
        {
            try
            {
                await _orchestrator.ForceNetworkScanAsync();
                return Ok(new { message = "Escaneo iniciado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error iniciando escaneo");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Fuerza el envío de un reporte
        /// </summary>
        [HttpPost("report")]
        public async Task<ActionResult> ForceReport()
        {
            try
            {
                await _orchestrator.ForceReportAsync();
                return Ok(new { message = "Reporte enviado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando reporte");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Procesa un comando del sistema central
        /// </summary>
        [HttpPost("commands")]
        public async Task<ActionResult<AgentCommandResponse>> ProcessCommand([FromBody] AgentCommand command)
        {
            try
            {
                if (command == null)
                {
                    return BadRequest(new { error = "Comando no válido" });
                }

                var response = await _orchestrator.ProcessCommandAsync(command);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando comando");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza la configuración del agente
        /// </summary>
        [HttpPut("configuration")]
        public async Task<ActionResult> UpdateConfiguration([FromBody] AgentConfiguration configuration)
        {
            try
            {
                if (configuration == null)
                {
                    return BadRequest(new { error = "Configuración no válida" });
                }

                await _orchestrator.UpdateConfigurationAsync(configuration);
                return Ok(new { message = "Configuración actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando configuración");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}

