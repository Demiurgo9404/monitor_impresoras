namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de procesamiento asíncrono y background jobs
    /// </summary>
    public interface IAsyncProcessingService
    {
        /// <summary>
        /// Encola trabajo para procesamiento asíncrono
        /// </summary>
        Task<string> EnqueueJobAsync(string jobType, string description, Func<IServiceProvider, CancellationToken, Task> jobAction,
            JobPriority priority = JobPriority.Normal, Dictionary<string, object>? parameters = null);

        /// <summary>
        /// Encola envío de notificaciones múltiples en background
        /// </summary>
        Task<string> EnqueueNotificationJobAsync(IEnumerable<string> recipients, string subject, string message,
            NotificationChannel channel, JobPriority priority = JobPriority.Normal);

        /// <summary>
        /// Encola generación de reportes en background
        /// </summary>
        Task<string> EnqueueReportGenerationJobAsync(string reportType, string userId, Dictionary<string, object>? parameters = null,
            JobPriority priority = JobPriority.Normal);

        /// <summary>
        /// Encola recolección de métricas en background
        /// </summary>
        Task<string> EnqueueMetricsCollectionJobAsync(JobPriority priority = JobPriority.Low);

        /// <summary>
        /// Obtiene estadísticas del servicio de procesamiento asíncrono
        /// </summary>
        AsyncProcessingStatistics GetProcessingStatistics();
    }
}
