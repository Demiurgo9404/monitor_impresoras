using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de mantenimiento predictivo
    /// </summary>
    public interface IPredictiveMaintenanceService
    {
        /// <summary>
        /// Entrena el modelo predictivo con datos históricos
        /// </summary>
        Task<TrainingResult> TrainModelAsync(IEnumerable<PrinterTelemetryClean> trainingData);

        /// <summary>
        /// Realiza predicciones de mantenimiento para una impresora
        /// </summary>
        Task<IEnumerable<MaintenancePrediction>> PredictMaintenanceAsync(int printerId, TimeSpan predictionWindow);

        /// <summary>
        /// Obtiene todas las predicciones recientes
        /// </summary>
        Task<IEnumerable<MaintenancePrediction>> GetRecentPredictionsAsync(int? printerId = null, DateTime? fromDate = null);
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
