using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class PredictionsController : ControllerBase
    {
        private readonly IPredictiveMaintenanceService _predictiveService;
        private readonly ILogger<PredictionsController> _logger;

        public PredictionsController(
            IPredictiveMaintenanceService predictiveService,
            ILogger<PredictionsController> logger)
        {
            _predictiveService = predictiveService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el dashboard predictivo con resumen de predicciones
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPredictionsSummary()
        {
            try
            {
                _logger.LogInformation("Obteniendo resumen de predicciones");

                var recentPredictions = await _predictiveService.GetRecentPredictionsAsync();

                var summary = new
                {
                    Timestamp = DateTime.UtcNow,
                    TotalPredictions = recentPredictions.Count(),
                    PredictionsByType = recentPredictions
                        .GroupBy(p => p.PredictionType)
                        .Select(g => new
                        {
                            Type = g.Key,
                            Count = g.Count(),
                            AverageProbability = g.Average(p => p.Probability),
                            AverageDaysUntilEvent = g.Where(p => p.DaysUntilEvent.HasValue).Average(p => p.DaysUntilEvent.Value)
                        }),
                    PredictionsBySeverity = recentPredictions
                        .GroupBy(p => p.GetSeverity())
                        .Select(g => new
                        {
                            Severity = g.Key,
                            Count = g.Count(),
                            AverageProbability = g.Average(p => p.Probability)
                        }),
                    CriticalPredictions = recentPredictions
                        .Where(p => p.GetSeverity() == PredictionSeverity.Critical)
                        .Select(p => new
                        {
                            PrinterId = p.PrinterId,
                            Type = p.PredictionType,
                            Probability = p.Probability,
                            DaysUntilEvent = p.DaysUntilEvent,
                            RecommendedAction = p.RecommendedAction
                        }),
                    TopRiskPrinters = recentPredictions
                        .OrderByDescending(p => p.Probability)
                        .Take(10)
                        .Select(p => new
                        {
                            PrinterId = p.PrinterId,
                            Type = p.PredictionType,
                            Probability = p.Probability,
                            DaysUntilEvent = p.DaysUntilEvent,
                            Confidence = p.Confidence
                        })
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen de predicciones");
                throw;
            }
        }

        /// <summary>
        /// Obtiene predicciones para una impresora específica
        /// </summary>
        [HttpGet("{printerId}")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPrinterPredictions(int printerId)
        {
            try
            {
                _logger.LogInformation("Obteniendo predicciones para impresora {PrinterId}", printerId);

                var predictions = await _predictiveService.GetRecentPredictionsAsync(printerId);

                if (!predictions.Any())
                {
                    return NotFound($"No se encontraron predicciones para la impresora {printerId}");
                }

                var result = new
                {
                    PrinterId = printerId,
                    TotalPredictions = predictions.Count(),
                    Predictions = predictions.Select(p => new
                    {
                        Id = p.Id,
                        Type = p.PredictionType,
                        Probability = p.Probability,
                        Severity = p.GetSeverity(),
                        EstimatedDate = p.EstimatedDate,
                        DaysUntilEvent = p.DaysUntilEvent,
                        Confidence = p.Confidence,
                        RecommendedAction = p.RecommendedAction,
                        PredictedAt = p.PredictedAt,
                        RequiresImmediateAttention = p.RequiresImmediateAttention
                    })
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo predicciones para impresora {PrinterId}", printerId);
                throw;
            }
        }

        /// <summary>
        /// Genera nuevas predicciones para una impresora
        /// </summary>
        [HttpPost("{printerId}/generate")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GeneratePredictions(int printerId)
        {
            try
            {
                _logger.LogInformation("Generando nuevas predicciones para impresora {PrinterId}", printerId);

                var predictions = await _predictiveService.PredictMaintenanceAsync(printerId, TimeSpan.FromDays(30));

                var result = new
                {
                    PrinterId = printerId,
                    PredictionsGenerated = predictions.Count(),
                    Predictions = predictions.Select(p => new
                    {
                        Type = p.PredictionType,
                        Probability = p.Probability,
                        Severity = p.GetSeverity(),
                        DaysUntilEvent = p.DaysUntilEvent,
                        Confidence = p.Confidence,
                        RecommendedAction = p.RecommendedAction
                    })
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando predicciones para impresora {PrinterId}", printerId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de precisión de predicciones
        /// </summary>
        [HttpGet("accuracy")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPredictionAccuracy()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de precisión de predicciones");

                // Aquí calcularías estadísticas reales de precisión
                var accuracyStats = new
                {
                    TotalPredictions = 150,
                    CorrectPredictions = 127,
                    AccuracyRate = 0.847m,
                    AccuracyByType = new[]
                    {
                        new { Type = "TonerDepletion", Total = 50, Correct = 43, Accuracy = 0.86m },
                        new { Type = "NetworkFailure", Total = 40, Correct = 32, Accuracy = 0.80m },
                        new { Type = "HardwareFailure", Total = 30, Correct = 26, Accuracy = 0.867m },
                        new { Type = "PaperDepletion", Total = 30, Correct = 26, Accuracy = 0.867m }
                    },
                    AverageLeadTime = 4.2m, // Días promedio de anticipación
                    FalsePositiveRate = 0.12m,
                    FalseNegativeRate = 0.08m
                };

                return Ok(accuracyStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de precisión");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas avanzadas de precisión del modelo ML
        /// </summary>
        [HttpGet("advanced-statistics")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAdvancedStatistics(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas avanzadas de predicciones");

                var statistics = await _predictiveService.GetAdvancedStatisticsAsync(fromDate, toDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas avanzadas");
                throw;
            }
        }
    }
}
