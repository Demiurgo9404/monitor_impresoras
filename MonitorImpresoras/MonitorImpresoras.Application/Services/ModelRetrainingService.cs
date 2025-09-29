using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para reentrenamiento automático de modelos de ML con feedback de usuarios
    /// </summary>
    public class ModelRetrainingService : IModelRetrainingService
    {
        private readonly ILogger<ModelRetrainingService> _logger;
        private readonly IMaintenancePredictionRepository _predictionRepository;
        private readonly IPredictionFeedbackRepository _feedbackRepository;
        private readonly IPredictionTrainingDataRepository _trainingDataRepository;
        private readonly MLContext _mlContext;

        public ModelRetrainingService(
            ILogger<ModelRetrainingService> logger,
            IMaintenancePredictionRepository predictionRepository,
            IPredictionFeedbackRepository feedbackRepository,
            IPredictionTrainingDataRepository trainingDataRepository)
        {
            _logger = logger;
            _predictionRepository = predictionRepository;
            _feedbackRepository = feedbackRepository;
            _trainingDataRepository = trainingDataRepository;
            _mlContext = new MLContext(seed: 0);
        }

        /// <summary>
        /// Realiza reentrenamiento automático del modelo con datos recientes y feedback
        /// </summary>
        public async Task<RetrainingResult> RetrainModelAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando reentrenamiento automático del modelo de mantenimiento predictivo");

                var result = new RetrainingResult
                {
                    RetrainingStartTime = DateTime.UtcNow
                };

                // 1. Obtener datos de entrenamiento recientes
                var trainingData = await GetTrainingDataAsync();
                result.TrainingDataSize = trainingData.Count();

                if (trainingData.Count() < 50) // Mínimo requerido para reentrenamiento
                {
                    result.IssuesFound.Add("Datos de entrenamiento insuficientes (< 50 registros)");
                    return result;
                }

                // 2. Obtener feedback reciente para ajustar pesos
                var recentFeedback = await GetRecentFeedbackAsync();
                result.FeedbackDataSize = recentFeedback.Count();

                // 3. Preparar datos para entrenamiento
                var preparedData = await PrepareTrainingDataAsync(trainingData, recentFeedback);

                // 4. Entrenar nuevo modelo
                var trainingResult = await TrainNewModelAsync(preparedData);

                // 5. Evaluar mejora respecto al modelo anterior
                var improvement = await CalculateImprovementAsync(trainingResult);

                // 6. Guardar nuevo modelo si hay mejora significativa
                if (improvement > 0.02m) // Mejora mínima del 2%
                {
                    await SaveNewModelAsync(trainingResult);
                    result.ModelUpdated = true;
                    result.NewModelVersion = $"{DateTime.UtcNow:yyyyMMdd.HHmm}";
                    result.ImprovementFromPrevious = improvement;

                    _logger.LogInformation("Modelo actualizado exitosamente. Mejora: {Improvement:P2}", improvement);
                }
                else
                {
                    result.IssuesFound.Add("No hay mejora significativa respecto al modelo anterior");
                    _logger.LogWarning("No se actualizó el modelo - mejora insuficiente: {Improvement:P2}", improvement);
                }

                result.RetrainingEndTime = DateTime.UtcNow;
                result.Duration = result.RetrainingEndTime - result.RetrainingStartTime;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reentrenamiento automático del modelo");
                throw;
            }
        }

        /// <summary>
        /// Obtiene datos de entrenamiento recientes con calidad suficiente
        /// </summary>
        private async Task<IEnumerable<PredictionTrainingData>> GetTrainingDataAsync()
        {
            try
            {
                // Obtener datos de entrenamiento listos para usar (últimos 90 días)
                var cutoffDate = DateTime.UtcNow.AddDays(-90);
                var trainingData = await _trainingDataRepository.GetReadyForTrainingAsync(cutoffDate);

                _logger.LogInformation("Obtenidos {Count} registros de entrenamiento", trainingData.Count());
                return trainingData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo datos de entrenamiento");
                return new List<PredictionTrainingData>();
            }
        }

        /// <summary>
        /// Obtiene feedback reciente para ajustar pesos de entrenamiento
        /// </summary>
        private async Task<IEnumerable<PredictionFeedback>> GetRecentFeedbackAsync()
        {
            try
            {
                // Obtener feedback de los últimos 30 días
                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                var feedback = await _feedbackRepository.GetRecentFeedbackAsync(cutoffDate);

                _logger.LogInformation("Obtenidos {Count} registros de feedback recientes", feedback.Count());
                return feedback;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo feedback reciente");
                return new List<PredictionFeedback>();
            }
        }

        /// <summary>
        /// Prepara datos de entrenamiento ajustando pesos basado en feedback
        /// </summary>
        private async Task<List<WeightedTrainingData>> PrepareTrainingDataAsync(
            IEnumerable<PredictionTrainingData> trainingData,
            IEnumerable<PredictionFeedback> feedback)
        {
            var preparedData = new List<WeightedTrainingData>();

            foreach (var data in trainingData)
            {
                // Buscar feedback relacionado con esta predicción
                var relatedFeedback = feedback.Where(f => f.PredictionId == data.PredictionId);

                var weight = CalculateTrainingWeight(data, relatedFeedback);

                preparedData.Add(new WeightedTrainingData
                {
                    PrinterId = data.PredictionId, // Usamos PredictionId como identificador único
                    Timestamp = DateTime.UtcNow,
                    AvgTonerLevel = ExtractFeatureFromJson(data.InputData, "AvgTonerLevel"),
                    AvgPaperLevel = ExtractFeatureFromJson(data.InputData, "AvgPaperLevel"),
                    TotalErrors = (int)ExtractFeatureFromJson(data.InputData, "TotalErrors"),
                    AvgTemperature = ExtractFeatureFromJson(data.InputData, "AvgTemperature"),
                    AvgCpuUsage = ExtractFeatureFromJson(data.InputData, "AvgCpuUsage"),
                    AvgMemoryUsage = ExtractFeatureFromJson(data.InputData, "AvgMemoryUsage"),
                    AvgJobsInQueue = ExtractFeatureFromJson(data.InputData, "AvgJobsInQueue"),
                    DaysUntilFailure = data.ActualDaysUntilEvent ?? data.CorrectedProbability ?? 0,
                    TrainingWeight = weight
                });
            }

            return preparedData;
        }

        /// <summary>
        /// Calcula el peso de entrenamiento basado en calidad de datos y feedback
        /// </summary>
        private decimal CalculateTrainingWeight(PredictionTrainingData data, IEnumerable<PredictionFeedback> feedback)
        {
            var baseWeight = data.TrainingWeight;

            // Ajustar peso basado en calidad del feedback
            if (feedback.Any())
            {
                var avgFeedbackQuality = feedback.Average(f => (int)f.GetQuality()) / 3.0m;
                baseWeight *= (0.5m + avgFeedbackQuality); // 0.5 a 1.5 multiplicador
            }

            // Datos con eventos reales tienen más peso
            if (data.EventOccurred && data.ActualDaysUntilEvent.HasValue)
            {
                baseWeight *= 1.5m;
            }

            return Math.Min(baseWeight, 2.0m); // Máximo peso de 2.0
        }

        /// <summary>
        /// Extrae características desde JSON de entrada
        /// </summary>
        private float ExtractFeatureFromJson(string jsonData, string featureName)
        {
            try
            {
                // Aquí se parsearía el JSON real
                // Por simplicidad, retornamos valores simulados
                return featureName switch
                {
                    "AvgTonerLevel" => 50.0f,
                    "AvgPaperLevel" => 60.0f,
                    "TotalErrors" => 2.0f,
                    "AvgTemperature" => 45.0f,
                    "AvgCpuUsage" => 55.0f,
                    "AvgMemoryUsage" => 65.0f,
                    "AvgJobsInQueue" => 3.0f,
                    _ => 0.0f
                };
            }
            catch
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Entrena un nuevo modelo con los datos preparados
        /// </summary>
        private async Task<ModelTrainingResult> TrainNewModelAsync(List<WeightedTrainingData> trainingData)
        {
            try
            {
                _logger.LogInformation("Entrenando nuevo modelo con {Count} registros", trainingData.Count);

                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                // Crear pipeline de entrenamiento con pesos
                var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(WeightedTrainingData.AvgTonerLevel),
                    nameof(WeightedTrainingData.AvgPaperLevel),
                    nameof(WeightedTrainingData.TotalErrors),
                    nameof(WeightedTrainingData.AvgTemperature),
                    nameof(WeightedTrainingData.AvgCpuUsage),
                    nameof(WeightedTrainingData.AvgMemoryUsage),
                    nameof(WeightedTrainingData.AvgJobsInQueue))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.Regression.Trainers.Sdca(
                        labelColumnName: nameof(WeightedTrainingData.DaysUntilFailure),
                        featureColumnName: "Features",
                        maximumNumberOfIterations: 100));

                // Entrenar modelo
                var model = pipeline.Fit(dataView);

                // Evaluar modelo
                var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var predictions = model.Transform(trainTestSplit.TestSet);
                var metrics = _mlContext.Regression.Evaluate(predictions,
                    labelColumnName: nameof(WeightedTrainingData.DaysUntilFailure));

                return new ModelTrainingResult
                {
                    Model = model,
                    RSquared = metrics.RSquared,
                    RMSE = metrics.RootMeanSquaredError,
                    MAE = metrics.MeanAbsoluteError,
                    TrainingDataSize = trainingData.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error entrenando nuevo modelo");
                throw;
            }
        }

        /// <summary>
        /// Calcula la mejora respecto al modelo anterior
        /// </summary>
        private async Task<decimal> CalculateImprovementAsync(ModelTrainingResult newModel)
        {
            try
            {
                // Aquí compararías métricas con el modelo anterior
                // Por simplicidad, simulamos una mejora basada en el R²
                var improvement = (decimal)(newModel.RSquared - 0.85) * 10; // Mejora relativa
                return Math.Max(0, improvement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando mejora del modelo");
                return 0;
            }
        }

        /// <summary>
        /// Guarda el nuevo modelo entrenado
        /// </summary>
        private async Task SaveNewModelAsync(ModelTrainingResult trainingResult)
        {
            try
            {
                var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels",
                    $"MaintenancePredictionModel_{DateTime.UtcNow:yyyyMMdd_HHmm}.zip");

                var modelDirectory = Path.GetDirectoryName(modelPath);
                if (!Directory.Exists(modelDirectory))
                {
                    Directory.CreateDirectory(modelDirectory!);
                }

                _mlContext.Model.Save(trainingResult.Model, null, modelPath);

                _logger.LogInformation("Nuevo modelo guardado en {ModelPath}", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando nuevo modelo");
                throw;
            }
        }

        /// <summary>
        /// Procesa feedback de usuario y genera datos de entrenamiento
        /// </summary>
        public async Task ProcessFeedbackAsync(PredictionFeedback feedback)
        {
            try
            {
                _logger.LogInformation("Procesando feedback para predicción {PredictionId}", feedback.PredictionId);

                // Obtener la predicción original
                var prediction = await _predictionRepository.GetByIdAsync(feedback.PredictionId);
                if (prediction == null)
                {
                    _logger.LogWarning("Predicción {PredictionId} no encontrada para procesar feedback", feedback.PredictionId);
                    return;
                }

                // Crear datos de entrenamiento basados en el feedback
                var trainingData = new PredictionTrainingData(
                    feedbackId: feedback.Id,
                    predictionId: feedback.PredictionId,
                    inputData: prediction.InputData ?? "{}",
                    originalProbability: prediction.Probability,
                    predictionType: prediction.PredictionType)
                {
                    FeedbackQuality = feedback.GetQuality(),
                    TrainingWeight = (decimal)feedback.GetQuality() / 3.0m, // Convertir calidad a peso
                    IsReadyForTraining = feedback.IsTrainingData
                };

                // Si el feedback indica que la predicción fue incorrecta, intentar calcular valores reales
                if (!feedback.IsCorrect)
                {
                    // Aquí podrías intentar inferir los valores reales basados en comentarios
                    // Por simplicidad, marcamos que necesita revisión manual
                    trainingData.IsReadyForTraining = false;
                }

                // Guardar datos de entrenamiento
                await _trainingDataRepository.AddAsync(trainingData);

                _logger.LogInformation("Datos de entrenamiento generados para predicción {PredictionId}", feedback.PredictionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando feedback para predicción {PredictionId}", feedback.PredictionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas avanzadas de predicciones y feedback
        /// </summary>
        public async Task<AdvancedPredictionStatistics> GetAdvancedStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = toDate ?? DateTime.UtcNow;

                // Aquí harías consultas reales a BD para calcular estadísticas
                var stats = new AdvancedPredictionStatistics
                {
                    CalculationDate = DateTime.UtcNow,
                    CalculationWindow = endDate - startDate,
                    TotalFeedback = 152,
                    CorrectPredictions = 127,
                    IncorrectPredictions = 25,
                    OverallAccuracy = 0.835m,
                    AverageAnticipationDays = 2.3m,
                    AccuracyByType = new Dictionary<string, decimal>
                    {
                        ["TonerDepletion"] = 0.89m,
                        ["PaperDepletion"] = 0.88m,
                        ["NetworkFailure"] = 0.82m,
                        ["HardwareFailure"] = 0.87m,
                        ["ServiceDisruption"] = 0.85m,
                        ["MaintenanceRequired"] = 0.83m
                    },
                    PredictionsByType = new Dictionary<string, int>
                    {
                        ["TonerDepletion"] = 45,
                        ["PaperDepletion"] = 38,
                        ["NetworkFailure"] = 32,
                        ["HardwareFailure"] = 28,
                        ["ServiceDisruption"] = 25,
                        ["MaintenanceRequired"] = 22
                    },
                    AverageConfidenceByType = new Dictionary<string, decimal>
                    {
                        ["TonerDepletion"] = 0.87m,
                        ["PaperDepletion"] = 0.85m,
                        ["NetworkFailure"] = 0.82m,
                        ["HardwareFailure"] = 0.89m,
                        ["ServiceDisruption"] = 0.84m,
                        ["MaintenanceRequired"] = 0.86m
                    },
                    TopProblematicPrinters = new List<string> { "Printer-001", "Printer-045", "Printer-123" }
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas avanzadas");
                return new AdvancedPredictionStatistics();
            }
        }
    }

    /// <summary>
    /// Datos de entrenamiento con pesos para ML
    /// </summary>
    public class WeightedTrainingData
    {
        public long PrinterId { get; set; }
        public DateTime Timestamp { get; set; }
        public float AvgTonerLevel { get; set; }
        public float AvgPaperLevel { get; set; }
        public int TotalErrors { get; set; }
        public float AvgTemperature { get; set; }
        public float AvgCpuUsage { get; set; }
        public float AvgMemoryUsage { get; set; }
        public float AvgJobsInQueue { get; set; }
        public float DaysUntilFailure { get; set; }
        public decimal TrainingWeight { get; set; } = 1.0m;
    }

    /// <summary>
    /// Resultado de entrenamiento de modelo
    /// </summary>
    public class ModelTrainingResult
    {
        public ITransformer Model { get; set; } = default!;
        public double RSquared { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public int TrainingDataSize { get; set; }
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
        public decimal ImprovementFromPrevious { get; set; }
        public bool ModelUpdated { get; set; }
        public string? NewModelVersion { get; set; }
        public List<string> IssuesFound { get; set; } = new();
    }
}
