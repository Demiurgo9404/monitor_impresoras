using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para almacenar feedback de usuarios sobre predicciones de mantenimiento
    /// </summary>
    [Table("PredictionFeedback")]
    public class PredictionFeedback
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// ID de la predicción que se está evaluando
        /// </summary>
        [Required]
        public long PredictionId { get; set; }

        /// <summary>
        /// Si la predicción fue correcta o no
        /// </summary>
        [Required]
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Comentarios del usuario sobre la predicción
        /// </summary>
        [MaxLength(1000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Corrección propuesta por el usuario (si aplica)
        /// </summary>
        [MaxLength(200)]
        public string? ProposedCorrection { get; set; }

        /// <summary>
        /// Usuario que proporcionó el feedback
        /// </summary>
        [Required, MaxLength(450)]
        public string CreatedBy { get; set; } = default!;

        /// <summary>
        /// Fecha de creación del feedback
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Información adicional del contexto del feedback
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? AdditionalContext { get; set; }

        /// <summary>
        /// Si el feedback fue revisado por un supervisor
        /// </summary>
        public bool SupervisorReviewed { get; set; }

        /// <summary>
        /// Usuario que revisó el feedback
        /// </summary>
        [MaxLength(450)]
        public string? ReviewedBy { get; set; }

        /// <summary>
        /// Comentarios de la revisión del supervisor
        /// </summary>
        [MaxLength(1000)]
        public string? ReviewComments { get; set; }

        /// <summary>
        /// Fecha de revisión por supervisor
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Si el feedback fue marcado como útil para el entrenamiento
        /// </summary>
        public bool IsTrainingData { get; set; }

        /// <summary>
        /// Peso del feedback para entrenamiento (0-1, basado en calidad)
        /// </summary>
        public decimal TrainingWeight { get; set; } = 1.0m;

        /// <summary>
        /// Navegación a la predicción
        /// </summary>
        [ForeignKey("PredictionId")]
        public virtual MaintenancePrediction? Prediction { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public PredictionFeedback() { }

        /// <summary>
        /// Constructor con parámetros principales
        /// </summary>
        public PredictionFeedback(long predictionId, bool isCorrect, string createdBy)
        {
            PredictionId = predictionId;
            IsCorrect = isCorrect;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Calcula el tiempo transcurrido desde la creación
        /// </summary>
        public TimeSpan TimeSinceCreation => DateTime.UtcNow - CreatedAt;

        /// <summary>
        /// Verifica si el feedback es reciente (menos de 24 horas)
        /// </summary>
        public bool IsRecent => TimeSinceCreation.TotalHours < 24;

        /// <summary>
        /// Obtiene la calidad del feedback basada en detalles proporcionados
        /// </summary>
        public FeedbackQuality GetQuality()
        {
            if (string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(ProposedCorrection))
                return FeedbackQuality.Low;

            if (!string.IsNullOrEmpty(Comment) && Comment.Length > 50)
                return FeedbackQuality.High;

            return FeedbackQuality.Medium;
        }

        /// <summary>
        /// Convierte el feedback a formato de entrenamiento para ML
        /// </summary>
        public TrainingFeedbackData ToTrainingData(MaintenancePrediction prediction)
        {
            return new TrainingFeedbackData
            {
                PredictionId = PredictionId,
                PredictionType = prediction.PredictionType,
                OriginalProbability = prediction.Probability,
                IsCorrect = IsCorrect,
                FeedbackQuality = GetQuality(),
                TrainingWeight = TrainingWeight,
                Comment = Comment,
                ProposedCorrection = ProposedCorrection,
                CreatedAt = CreatedAt
            };
        }
    }

    /// <summary>
    /// Entidad para datos de entrenamiento derivados del feedback
    /// </summary>
    [Table("PredictionTrainingData")]
    public class PredictionTrainingData
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// ID del feedback que generó este dato de entrenamiento
        /// </summary>
        [Required]
        public long FeedbackId { get; set; }

        /// <summary>
        /// ID de la predicción original
        /// </summary>
        [Required]
        public long PredictionId { get; set; }

        /// <summary>
        /// Datos de entrada originales de la predicción
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public string InputData { get; set; } = default!;

        /// <summary>
        /// Probabilidad original predicha
        /// </summary>
        [Required]
        public decimal OriginalProbability { get; set; }

        /// <summary>
        /// Probabilidad corregida basada en el feedback
        /// </summary>
        public decimal? CorrectedProbability { get; set; }

        /// <summary>
        /// Fecha real del evento (si se conoce)
        /// </summary>
        public DateTime? ActualEventDate { get; set; }

        /// <summary>
        /// Días reales hasta el evento
        /// </summary>
        public int? ActualDaysUntilEvent { get; set; }

        /// <summary>
        /// Si el evento ocurrió realmente
        /// </summary>
        public bool EventOccurred { get; set; }

        /// <summary>
        /// Tipo de predicción
        /// </summary>
        [Required, MaxLength(50)]
        public string PredictionType { get; set; } = default!;

        /// <summary>
        /// Calidad del feedback usado para generar este dato
        /// </summary>
        [Required]
        public FeedbackQuality FeedbackQuality { get; set; }

        /// <summary>
        /// Peso del dato para entrenamiento (0-1)
        /// </summary>
        public decimal TrainingWeight { get; set; } = 1.0m;

        /// <summary>
        /// Si el dato está listo para ser usado en entrenamiento
        /// </summary>
        public bool IsReadyForTraining { get; set; }

        /// <summary>
        /// Fecha de creación del dato de entrenamiento
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navegación al feedback
        /// </summary>
        [ForeignKey("FeedbackId")]
        public virtual PredictionFeedback? Feedback { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public PredictionTrainingData() { }

        /// <summary>
        /// Constructor con parámetros principales
        /// </summary>
        public PredictionTrainingData(long feedbackId, long predictionId, string inputData, decimal originalProbability, string predictionType)
        {
            FeedbackId = feedbackId;
            PredictionId = predictionId;
            InputData = inputData;
            OriginalProbability = originalProbability;
            PredictionType = predictionType;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Calcula el error de la predicción (días de diferencia)
        /// </summary>
        public int? CalculatePredictionError()
        {
            if (!ActualDaysUntilEvent.HasValue || !CorrectedProbability.HasValue)
                return null;

            // Aquí se calcularía el error basado en la diferencia entre predicción y realidad
            return ActualDaysUntilEvent.Value;
        }

        /// <summary>
        /// Verifica si el dato tiene suficiente calidad para entrenamiento
        /// </summary>
        public bool HasHighQuality => TrainingWeight >= 0.7m && FeedbackQuality >= FeedbackQuality.Medium;
    }

    /// <summary>
    /// Calidad del feedback proporcionado
    /// </summary>
    public enum FeedbackQuality
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// DTO para datos de entrenamiento derivados del feedback
    /// </summary>
    public class TrainingFeedbackData
    {
        public long PredictionId { get; set; }
        public string PredictionType { get; set; } = string.Empty;
        public decimal OriginalProbability { get; set; }
        public bool IsCorrect { get; set; }
        public FeedbackQuality FeedbackQuality { get; set; }
        public decimal TrainingWeight { get; set; }
        public string? Comment { get; set; }
        public string? ProposedCorrection { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas avanzadas de predicciones
    /// </summary>
    public class AdvancedPredictionStatistics
    {
        public Dictionary<string, decimal> AccuracyByType { get; set; } = new();
        public decimal OverallAccuracy { get; set; }
        public decimal AverageAnticipationDays { get; set; }
        public int TotalFeedback { get; set; }
        public int CorrectPredictions { get; set; }
        public int IncorrectPredictions { get; set; }
        public Dictionary<string, int> PredictionsByType { get; set; } = new();
        public Dictionary<string, decimal> AverageConfidenceByType { get; set; } = new();
        public List<string> TopProblematicPrinters { get; set; } = new();
        public DateTime CalculationDate { get; set; }
        public TimeSpan CalculationWindow { get; set; }
    }

    /// <summary>
    /// DTO para resultado de reentrenamiento
    /// </summary>
    public class RetrainingResult
    {
        public DateTime RetrainingStartTime { get; set; }
        public DateTime RetrainingEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int TrainingDataSize { get; set; }
        public int FeedbackDataSize { get; set; }
        public double NewModelRSquared { get; set; }
        public double NewModelRMSE { get; set; }
        public double ImprovementFromPrevious { get; set; }
        public bool ModelUpdated { get; set; }
        public string? ModelVersion { get; set; }
        public List<string> IssuesFound { get; set; } = new();
    }
}
