using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio de predicciones usando Machine Learning
    /// </summary>
    public class PredictiveMaintenanceService : IPredictiveMaintenanceService
    {
        private readonly IModelRetrainingService _retrainingService;
        private readonly IPredictionFeedbackRepository _feedbackRepository;
        private readonly ILogger<PredictiveMaintenanceService> _logger;
        private ITransformer? _trainedModel;
        private string _modelPath;

        public PredictiveMaintenanceService(
            ILogger<PredictiveMaintenanceService> logger,
            IModelRetrainingService retrainingService,
            IPredictionFeedbackRepository feedbackRepository)
        {
            _logger = logger;
            _retrainingService = retrainingService;
            _feedbackRepository = feedbackRepository;
            _mlContext = new MLContext(seed: 0);
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels", "MaintenancePredictionModel.zip");
        }

        /// <summary>
        /// Entrena el modelo predictivo con datos históricos
        /// </summary>
        public async Task<TrainingResult> TrainModelAsync(IEnumerable<PrinterTelemetryClean> trainingData)
        {
            try
            {
                _logger.LogInformation("Iniciando entrenamiento del modelo predictivo con {Count} registros", trainingData.Count());

                var result = new TrainingResult
                {
                    TrainingStartTime = DateTime.UtcNow,
                    TrainingDataSize = trainingData.Count()
                };

                // Preparar datos para ML.NET
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData.Select(MapToTrainingData));

                // Dividir datos en entrenamiento y prueba
                var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainingDataView = trainTestSplit.TrainSet;
                var testDataView = trainTestSplit.TestSet;

                // Crear pipeline de transformación
                var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(TrainingData.AvgTonerLevel),
                    nameof(TrainingData.AvgPaperLevel),
                    nameof(TrainingData.TotalErrors),
                    nameof(TrainingData.AvgTemperature),
                    nameof(TrainingData.AvgCpuUsage),
                    nameof(TrainingData.AvgMemoryUsage),
                    nameof(TrainingData.AvgJobsInQueue))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.Regression.Trainers.Sdca(
                        labelColumnName: nameof(TrainingData.DaysUntilFailure),
                        featureColumnName: "Features"));

                // Entrenar modelo
                _trainedModel = pipeline.Fit(trainingDataView);

                // Evaluar modelo
                var predictions = _trainedModel.Transform(testDataView);
                var metrics = _mlContext.Regression.Evaluate(predictions,
                    labelColumnName: nameof(TrainingData.DaysUntilFailure));

                result.RSquared = metrics.RSquared;
                result.RootMeanSquaredError = metrics.RootMeanSquaredError;
                result.MeanAbsoluteError = metrics.MeanAbsoluteError;
                result.TrainingEndTime = DateTime.UtcNow;
                result.Duration = result.TrainingEndTime - result.TrainingStartTime;

                // Guardar modelo entrenado
                await SaveModelAsync();

                _logger.LogInformation("Modelo entrenado exitosamente. R²: {RSquared:F4}, RMSE: {RMSE:F2}",
                    result.RSquared, result.RootMeanSquaredError);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error entrenando modelo predictivo");
                throw;
            }
        }

        /// <summary>
        /// Realiza predicciones de mantenimiento para una impresora
        /// </summary>
        public async Task<IEnumerable<MaintenancePrediction>> PredictMaintenanceAsync(int printerId, TimeSpan predictionWindow)
        {
            try
            {
                if (_trainedModel == null)
                {
                    await LoadModelAsync();
                }

                _logger.LogInformation("Generando predicciones para impresora {PrinterId}", printerId);

                // Aquí obtendrías datos recientes de telemetría limpios
                // var recentData = await GetRecentCleanDataAsync(printerId);

                // Por simplicidad, simulamos datos recientes
                var recentData = GenerateSampleRecentData(printerId);

                var predictions = new List<MaintenancePrediction>();

                // Generar predicciones para diferentes tipos de fallo
                var predictionTypes = new[]
                {
                    PredictionType.TonerDepletion,
                    PredictionType.PaperDepletion,
                    PredictionType.NetworkFailure,
                    PredictionType.HardwareFailure
                };

                foreach (var predictionType in predictionTypes)
                {
                    var prediction = await GeneratePredictionAsync(printerId, predictionType, recentData);
                    if (prediction != null)
                    {
                        predictions.Add(prediction);
                    }
                }

                // Guardar predicciones en BD
                // await _maintenanceRepository.AddPredictionsAsync(predictions);

                _logger.LogInformation("Generadas {Count} predicciones para impresora {PrinterId}",
                    predictions.Count, printerId);

                return predictions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando predicciones para impresora {PrinterId}", printerId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las predicciones recientes
        /// </summary>
        public async Task<IEnumerable<MaintenancePrediction>> GetRecentPredictionsAsync(int? printerId = null, DateTime? fromDate = null)
        {
            try
            {
                // Aquí harías consultas reales a BD
                var startDate = fromDate ?? DateTime.UtcNow.AddDays(-7);

                // Simulación de predicciones recientes
                return new List<MaintenancePrediction>
                {
                    new MaintenancePrediction(1, PredictionType.TonerDepletion.ToString(), 0.85m)
                    {
                        PredictedAt = DateTime.UtcNow.AddHours(-2),
                        EstimatedDate = DateTime.UtcNow.AddDays(3),
                        DaysUntilEvent = 3,
                        Confidence = 0.87m,
                        RecommendedAction = "Reemplazar tóner en los próximos 2-3 días"
                    },
                    new MaintenancePrediction(2, PredictionType.NetworkFailure.ToString(), 0.65m)
                    {
                        PredictedAt = DateTime.UtcNow.AddHours(-1),
                        EstimatedDate = DateTime.UtcNow.AddDays(5),
                        DaysUntilEvent = 5,
                        Confidence = 0.72m,
                        RecommendedAction = "Verificar conectividad de red"
                    }
                }.Where(p => !printerId.HasValue || p.PrinterId == printerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo predicciones recientes");
                return new List<MaintenancePrediction>();
            }
        }

        /// <summary>
        /// Genera una predicción específica para un tipo de fallo
        /// </summary>
        private async Task<MaintenancePrediction?> GeneratePredictionAsync(
            int printerId,
            PredictionType predictionType,
            PrinterTelemetryClean recentData)
        {
            try
            {
                var predictionData = MapToPredictionData(recentData, predictionType);

                var predictionEngine = _mlContext.Model.CreatePredictionEngine<TrainingData, PredictionOutput>(_trainedModel);
                var prediction = predictionEngine.Predict(predictionData);

                // Solo crear predicción si hay alta probabilidad de fallo
                if (prediction.Score <= 0)
                    return null;

                var maintenancePrediction = new MaintenancePrediction(printerId, predictionType.ToString(), (decimal)prediction.Score)
                {
                    PredictedAt = DateTime.UtcNow,
                    EstimatedDate = DateTime.UtcNow.AddDays(prediction.Score),
                    DaysUntilEvent = (int)Math.Ceiling(prediction.Score),
                    Confidence = CalculateConfidence(prediction.Score),
                    ModelVersion = "1.0.0",
                    InputData = System.Text.Json.JsonSerializer.Serialize(predictionData),
                    RecommendedAction = GetRecommendedAction(predictionType, prediction.Score)
                };

                return maintenancePrediction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando predicción {PredictionType} para impresora {PrinterId}",
                    predictionType, printerId);
                return null;
            }
        }

        /// <summary>
        /// Calcula la confianza basada en la probabilidad
        /// </summary>
        private decimal CalculateConfidence(double probability)
        {
            return probability switch
            {
                >= 0.8 => 0.9m,
                >= 0.6 => 0.7m,
                >= 0.4 => 0.5m,
                _ => 0.3m
            };
        }

        /// <summary>
        /// Obtiene la acción recomendada basada en el tipo de predicción
        /// </summary>
        private string GetRecommendedAction(PredictionType predictionType, double probability)
        {
            return predictionType switch
            {
                PredictionType.TonerDepletion => probability >= 0.7
                    ? "Reemplazar tóner inmediatamente"
                    : "Planificar reemplazo de tóner en los próximos días",
                PredictionType.PaperDepletion => probability >= 0.7
                    ? "Reabastecer papel inmediatamente"
                    : "Verificar niveles de papel",
                PredictionType.NetworkFailure => probability >= 0.7
                    ? "Verificar conectividad de red urgentemente"
                    : "Monitorear conectividad de red",
                PredictionType.HardwareFailure => probability >= 0.7
                    ? "Llamar a servicio técnico inmediatamente"
                    : "Programar revisión técnica preventiva",
                _ => "Revisión general recomendada"
            };
        }

        /// <summary>
        /// Mapea datos de telemetría limpia a formato de entrenamiento
        /// </summary>
        private TrainingData MapToTrainingData(PrinterTelemetryClean telemetry)
        {
            return new TrainingData
            {
                PrinterId = telemetry.PrinterId,
                Timestamp = telemetry.TimestampUtc,
                AvgTonerLevel = telemetry.AvgTonerLevel ?? 0,
                AvgPaperLevel = telemetry.AvgPaperLevel ?? 0,
                TotalErrors = telemetry.TotalErrors ?? 0,
                AvgTemperature = telemetry.AvgTemperature ?? 0,
                AvgCpuUsage = telemetry.AvgCpuUsage ?? 0,
                AvgMemoryUsage = telemetry.AvgMemoryUsage ?? 0,
                AvgJobsInQueue = telemetry.AvgJobsInQueue ?? 0,
                DaysUntilFailure = 0 // Esto vendría de datos históricos reales
            };
        }

        /// <summary>
        /// Mapea datos recientes a formato de predicción
        /// </summary>
        private TrainingData MapToPredictionData(PrinterTelemetryClean telemetry, PredictionType predictionType)
        {
            return new TrainingData
            {
                PrinterId = telemetry.PrinterId,
                Timestamp = telemetry.TimestampUtc,
                AvgTonerLevel = telemetry.AvgTonerLevel ?? 0,
                AvgPaperLevel = telemetry.AvgPaperLevel ?? 0,
                TotalErrors = telemetry.TotalErrors ?? 0,
                AvgTemperature = telemetry.AvgTemperature ?? 0,
                AvgCpuUsage = telemetry.AvgCpuUsage ?? 0,
                AvgMemoryUsage = telemetry.AvgMemoryUsage ?? 0,
                AvgJobsInQueue = telemetry.AvgJobsInQueue ?? 0,
                DaysUntilFailure = 0
            };
        }

        /// <summary>
        /// Genera datos de muestra recientes para pruebas
        /// </summary>
        private PrinterTelemetryClean GenerateSampleRecentData(int printerId)
        {
            return new PrinterTelemetryClean
            {
                PrinterId = printerId,
                TimestampUtc = DateTime.UtcNow,
                AvgTonerLevel = 25.0m, // Tóner bajo
                AvgPaperLevel = 15.0m, // Papel bajo
                TotalErrors = 3,       // Algunos errores
                AvgTemperature = 45.0m,
                AvgCpuUsage = 60.0m,
                AvgMemoryUsage = 70.0m,
                AvgJobsInQueue = 2.0m,
                SampleCount = 10,
                DataQualityScore = 85.0m,
                DominantStatus = "Online"
            };
        }

        /// <summary>
        /// Procesa feedback de usuario sobre una predicción
        /// </summary>
        public async Task<bool> ProcessFeedbackAsync(long predictionId, bool isCorrect, string? comment, string userId)
        {
            try
            {
                _logger.LogInformation("Procesando feedback para predicción {PredictionId} por usuario {UserId}", predictionId, userId);

                var feedback = new PredictionFeedback(predictionId, isCorrect, userId)
                {
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };

                // Procesar feedback con el servicio de reentrenamiento
                await _retrainingService.ProcessFeedbackAsync(feedback);

                _logger.LogInformation("Feedback procesado exitosamente para predicción {PredictionId}", predictionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando feedback para predicción {PredictionId}", predictionId);
                return false;
            }
        }

        /// <summary>
        /// Obtiene estadísticas avanzadas de predicciones
        /// </summary>
        public async Task<AdvancedPredictionStatistics> GetAdvancedStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                return await _retrainingService.GetAdvancedStatisticsAsync(fromDate, toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas avanzadas");
                return new AdvancedPredictionStatistics();
            }
        }

        /// <summary>
        /// Realiza reentrenamiento automático del modelo
        /// </summary>
        public async Task<RetrainingResult> RetrainModelAsync()
        {
            try
            {
                return await _retrainingService.RetrainModelAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reentrenamiento automático del modelo");
                return new RetrainingResult();
            }
        }

        /// <summary>
        /// Carga el modelo entrenado desde disco
        /// </summary>
        private async Task LoadModelAsync()
        {
            try
            {
                if (File.Exists(_modelPath))
                {
                    _trainedModel = _mlContext.Model.Load(_modelPath, out _);
                    _logger.LogInformation("Modelo cargado desde {ModelPath}", _modelPath);
                }
                else
                {
                    _logger.LogWarning("Modelo no encontrado en {ModelPath}, se requerirá entrenamiento", _modelPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando modelo desde {ModelPath}", _modelPath);
            }
        }
    }

    /// <summary>
    /// Clase de datos para entrenamiento del modelo
    /// </summary>
    public class TrainingData
    {
        public int PrinterId { get; set; }
        public DateTime Timestamp { get; set; }
        public float AvgTonerLevel { get; set; }
        public float AvgPaperLevel { get; set; }
        public int TotalErrors { get; set; }
        public float AvgTemperature { get; set; }
        public float AvgCpuUsage { get; set; }
        public float AvgMemoryUsage { get; set; }
        public float AvgJobsInQueue { get; set; }

        // Variable objetivo (días hasta el fallo)
        public float DaysUntilFailure { get; set; }
    }

    /// <summary>
    /// Clase de salida de predicciones
    /// </summary>
    public class PredictionOutput
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }

    /// <summary>
    /// DTO para resultado de entrenamiento
    /// </summary>
    public class TrainingResult
    {
        public DateTime TrainingStartTime { get; set; }
        public DateTime TrainingEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int TrainingDataSize { get; set; }
        public double RSquared { get; set; }
        public double RootMeanSquaredError { get; set; }
        public double MeanAbsoluteError { get; set; }
    }
}
