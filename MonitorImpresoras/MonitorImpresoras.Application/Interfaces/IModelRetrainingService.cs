using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de reentrenamiento de modelos de ML
    /// </summary>
    public interface IModelRetrainingService
    {
        /// <summary>
        /// Realiza reentrenamiento automático del modelo con datos recientes y feedback
        /// </summary>
        Task<RetrainingResult> RetrainModelAsync();

        /// <summary>
        /// Procesa feedback de usuario y genera datos de entrenamiento
        /// </summary>
        Task ProcessFeedbackAsync(PredictionFeedback feedback);

        /// <summary>
        /// Obtiene estadísticas avanzadas de predicciones y feedback
        /// </summary>
        Task<AdvancedPredictionStatistics> GetAdvancedStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
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
        public decimal ImprovementFromPrevious { get; set; }
        public bool ModelUpdated { get; set; }
        public string? NewModelVersion { get; set; }
        public List<string> IssuesFound { get; set; } = new();
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
}
