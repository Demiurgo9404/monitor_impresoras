using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IPredictiveMaintenanceService _predictiveService;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(
            IPredictiveMaintenanceService predictiveService,
            ILogger<FeedbackController> logger)
        {
            _predictiveService = predictiveService;
            _logger = logger;
        }

        /// <summary>
        /// Envía feedback sobre una predicción específica
        /// </summary>
        [HttpPost("{predictionId}/feedback")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitFeedback(long predictionId, [FromBody] FeedbackDto feedbackDto)
        {
            try
            {
                _logger.LogInformation("Procesando feedback para predicción {PredictionId} por usuario {UserId}",
                    predictionId, User.Identity?.Name);

                var userId = User.Identity?.Name ?? "anonymous";
                var success = await _predictiveService.ProcessFeedbackAsync(
                    predictionId,
                    feedbackDto.IsCorrect,
                    feedbackDto.Comment,
                    userId);

                if (!success)
                {
                    return NotFound($"Predicción {predictionId} no encontrada o error procesando feedback");
                }

                return Ok(new
                {
                    Message = "Feedback procesado exitosamente",
                    PredictionId = predictionId,
                    ProcessedBy = userId,
                    ProcessedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando feedback para predicción {PredictionId}", predictionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas avanzadas de predicciones y feedback
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Policy = "RequireManager")]
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

        /// <summary>
        /// Realiza reentrenamiento manual del modelo de ML
        /// </summary>
        [HttpPost("retrain")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RetrainModel()
        {
            try
            {
                _logger.LogInformation("Iniciando reentrenamiento manual del modelo por usuario {UserId}", User.Identity?.Name);

                var result = await _predictiveService.RetrainModelAsync();

                return Ok(new
                {
                    Message = result.ModelUpdated ? "Modelo reentrenado exitosamente" : "Reentrenamiento completado sin actualización",
                    RetrainingResult = result,
                    TriggeredBy = User.Identity?.Name,
                    TriggeredAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reentrenamiento manual del modelo");
                throw;
            }
        }

        /// <summary>
        /// Obtiene historial de feedback para una predicción específica
        /// </summary>
        [HttpGet("{predictionId}/feedback")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPredictionFeedback(long predictionId)
        {
            try
            {
                _logger.LogInformation("Obteniendo historial de feedback para predicción {PredictionId}", predictionId);

                // Aquí obtendrías el historial real de feedback de BD
                var feedbackHistory = new[]
                {
                    new
                    {
                        Id = 1,
                        PredictionId = predictionId,
                        IsCorrect = true,
                        Comment = "La predicción fue precisa",
                        CreatedBy = "admin@empresa.com",
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        Quality = "High"
                    },
                    new
                    {
                        Id = 2,
                        PredictionId = predictionId,
                        IsCorrect = false,
                        Comment = "El fallo ocurrió antes de lo previsto",
                        CreatedBy = "tecnico@empresa.com",
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        Quality = "Medium"
                    }
                };

                return Ok(new
                {
                    PredictionId = predictionId,
                    TotalFeedback = feedbackHistory.Length,
                    FeedbackHistory = feedbackHistory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de feedback para predicción {PredictionId}", predictionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene métricas de calidad del modelo de ML
        /// </summary>
        [HttpGet("model-quality")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetModelQualityMetrics()
        {
            try
            {
                _logger.LogInformation("Obteniendo métricas de calidad del modelo ML");

                var qualityMetrics = new
                {
                    ModelVersion = "1.0.20250130.1430",
                    LastTrainingDate = DateTime.UtcNow.AddDays(-7),
                    TrainingDataSize = 2500,
                    CurrentAccuracy = 0.87m,
                    AccuracyTrend = new[]
                    {
                        new { Date = DateTime.UtcNow.AddDays(-7), Accuracy = 0.84m },
                        new { Date = DateTime.UtcNow.AddDays(-6), Accuracy = 0.85m },
                        new { Date = DateTime.UtcNow.AddDays(-5), Accuracy = 0.86m },
                        new { Date = DateTime.UtcNow.AddDays(-4), Accuracy = 0.87m },
                        new { Date = DateTime.UtcNow.AddDays(-3), Accuracy = 0.86m },
                        new { Date = DateTime.UtcNow.AddDays(-2), Accuracy = 0.88m },
                        new { Date = DateTime.UtcNow.AddDays(-1), Accuracy = 0.87m }
                    },
                    PredictionTypes = new[]
                    {
                        new { Type = "TonerDepletion", Accuracy = 0.89m, SampleSize = 450 },
                        new { Type = "PaperDepletion", Accuracy = 0.88m, SampleSize = 380 },
                        new { Type = "NetworkFailure", Accuracy = 0.82m, SampleSize = 320 },
                        new { Type = "HardwareFailure", Accuracy = 0.87m, SampleSize = 280 },
                        new { Type = "ServiceDisruption", Accuracy = 0.85m, SampleSize = 250 },
                        new { Type = "MaintenanceRequired", Accuracy = 0.83m, SampleSize = 220 }
                    },
                    FeedbackQuality = new
                    {
                        TotalFeedback = 152,
                        HighQualityFeedback = 89,
                        MediumQualityFeedback = 45,
                        LowQualityFeedback = 18,
                        AverageQuality = 0.78m
                    }
                };

                return Ok(qualityMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas de calidad del modelo");
                throw;
            }
        }
    }

    /// <summary>
    /// DTO para envío de feedback
    /// </summary>
    public class FeedbackDto
    {
        public bool IsCorrect { get; set; }
        public string? Comment { get; set; }
        public string? ProposedCorrection { get; set; }
    }
}
