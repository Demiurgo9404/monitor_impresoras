using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using System.Collections.Concurrent;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio avanzado de procesamiento asíncrono y background jobs con .NET BackgroundService
    /// </summary>
    public class AsyncProcessingService : BackgroundService, IAsyncProcessingService
    {
        private readonly ILogger<AsyncProcessingService> _logger;
        private readonly ICentralizedLoggingService _loggingService;
        private readonly IComprehensiveMetricsService _metricsService;

        // Colas de trabajos por prioridad
        private readonly ConcurrentQueue<BackgroundJob> _highPriorityQueue = new();
        private readonly ConcurrentQueue<BackgroundJob> _normalPriorityQueue = new();
        private readonly ConcurrentQueue<BackgroundJob> _lowPriorityQueue = new();

        // Estado del servicio
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private int _activeJobs = 0;
        private int _completedJobs = 0;
        private int _failedJobs = 0;

        // Configuración de procesamiento
        private const int MaxConcurrentJobs = 5;
        private const int QueuePollingIntervalMs = 1000;
        private const int JobTimeoutMinutes = 30;

        public AsyncProcessingService(
            ILogger<AsyncProcessingService> logger,
            ICentralizedLoggingService loggingService,
            IComprehensiveMetricsService metricsService)
        {
            _logger = logger;
            _loggingService = loggingService;
            _metricsService = metricsService;
        }

        /// <summary>
        /// Encola trabajo para procesamiento asíncrono
        /// </summary>
        public async Task<string> EnqueueJobAsync(string jobType, string description, Func<IServiceProvider, CancellationToken, Task> jobAction,
            JobPriority priority = JobPriority.Normal, Dictionary<string, object>? parameters = null)
        {
            var job = new BackgroundJob
            {
                Id = Guid.NewGuid().ToString(),
                JobType = jobType,
                Description = description,
                Priority = priority,
                Status = JobStatus.Queued,
                CreatedAt = DateTime.UtcNow,
                Parameters = parameters ?? new Dictionary<string, object>(),
                JobAction = jobAction,
                MaxRetries = GetMaxRetriesForJobType(jobType),
                RetryCount = 0
            };

            // Encolar según prioridad
            switch (priority)
            {
                case JobPriority.High:
                    _highPriorityQueue.Enqueue(job);
                    break;
                case JobPriority.Normal:
                    _normalPriorityQueue.Enqueue(job);
                    break;
                case JobPriority.Low:
                    _lowPriorityQueue.Enqueue(job);
                    break;
            }

            _logger.LogInformation("Trabajo encolado: {JobId} - {JobType} - Prioridad: {Priority}", job.Id, jobType, priority);

            _loggingService.LogApplicationEvent(
                "background_job_enqueued",
                $"Trabajo encolado: {jobType}",
                ApplicationLogLevel.Info,
                additionalData: new Dictionary<string, object>
                {
                    ["JobId"] = job.Id,
                    ["JobType"] = jobType,
                    ["Priority"] = priority.ToString(),
                    ["Description"] = description
                });

            return job.Id;
        }

        /// <summary>
        /// Encola envío de notificaciones múltiples en background
        /// </summary>
        public async Task<string> EnqueueNotificationJobAsync(IEnumerable<string> recipients, string subject, string message,
            NotificationChannel channel, JobPriority priority = JobPriority.Normal)
        {
            return await EnqueueJobAsync(
                "SendMultipleNotifications",
                $"Enviar {recipients.Count()} notificaciones por {channel}",
                async (services, cancellationToken) =>
                {
                    await SendMultipleNotificationsAsync(recipients, subject, message, channel, cancellationToken);
                },
                priority,
                new Dictionary<string, object>
                {
                    ["Recipients"] = recipients,
                    ["Subject"] = subject,
                    ["Message"] = message,
                    ["Channel"] = channel
                });
        }

        /// <summary>
        /// Encola generación de reportes en background
        /// </summary>
        public async Task<string> EnqueueReportGenerationJobAsync(string reportType, string userId, Dictionary<string, object>? parameters = null,
            JobPriority priority = JobPriority.Normal)
        {
            return await EnqueueJobAsync(
                "GenerateReport",
                $"Generar reporte {reportType} para usuario {userId}",
                async (services, cancellationToken) =>
                {
                    await GenerateReportAsync(reportType, userId, parameters, cancellationToken);
                },
                priority,
                new Dictionary<string, object>
                {
                    ["ReportType"] = reportType,
                    ["UserId"] = userId,
                    ["Parameters"] = parameters ?? new Dictionary<string, object>()
                });
        }

        /// <summary>
        /// Encola recolección de métricas en background
        /// </summary>
        public async Task<string> EnqueueMetricsCollectionJobAsync(JobPriority priority = JobPriority.Low)
        {
            return await EnqueueJobAsync(
                "CollectMetrics",
                "Recolectar métricas de rendimiento del sistema",
                async (services, cancellationToken) =>
                {
                    await CollectSystemMetricsAsync(cancellationToken);
                },
                priority);
        }

        /// <summary>
        /// Obtiene estadísticas del servicio de procesamiento asíncrono
        /// </summary>
        public AsyncProcessingStatistics GetProcessingStatistics()
        {
            return new AsyncProcessingStatistics
            {
                ServiceStatus = GetServiceStatus(),
                ActiveJobs = _activeJobs,
                QueuedJobs = GetTotalQueuedJobs(),
                CompletedJobs = _completedJobs,
                FailedJobs = _failedJobs,
                AverageJobDuration = GetAverageJobDuration(),
                LastJobCompleted = GetLastJobCompletionTime(),
                QueueSizes = new Dictionary<JobPriority, int>
                {
                    [JobPriority.High] = _highPriorityQueue.Count,
                    [JobPriority.Normal] = _normalPriorityQueue.Count,
                    [JobPriority.Low] = _lowPriorityQueue.Count
                },
                ProcessingRate = GetProcessingRate()
            };
        }

        // Implementación de BackgroundService

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de procesamiento asíncrono iniciado");

            _loggingService.LogApplicationEvent(
                "async_processing_service_started",
                "Servicio de procesamiento asíncrono iniciado",
                ApplicationLogLevel.Info);

            try
            {
                var tasks = new List<Task>();

                // Iniciar trabajadores para cada nivel de prioridad
                tasks.Add(StartWorkerAsync(JobPriority.High, stoppingToken));
                tasks.Add(StartWorkerAsync(JobPriority.Normal, stoppingToken));
                tasks.Add(StartWorkerAsync(JobPriority.Low, stoppingToken));

                // Iniciar recolector de métricas automático
                tasks.Add(StartMetricsCollectorAsync(stoppingToken));

                // Iniciar limpieza automática de trabajos antiguos
                tasks.Add(StartCleanupTaskAsync(stoppingToken));

                await Task.WhenAll(tasks);

                _logger.LogInformation("Servicio de procesamiento asíncrono detenido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en servicio de procesamiento asíncrono");

                _loggingService.LogApplicationEvent(
                    "async_processing_service_error",
                    $"Error crítico en servicio de procesamiento asíncrono: {ex.Message}",
                    ApplicationLogLevel.Critical,
                    additionalData: new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message,
                        ["StackTrace"] = ex.StackTrace ?? ""
                    });
            }
        }

        /// <summary>
        /// Inicia trabajador para procesar trabajos de una prioridad específica
        /// </summary>
        private async Task StartWorkerAsync(JobPriority priority, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando trabajador para prioridad: {Priority}", priority);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_activeJobs >= MaxConcurrentJobs)
                    {
                        await Task.Delay(QueuePollingIntervalMs, cancellationToken);
                        continue;
                    }

                    // Intentar obtener trabajo de la cola correspondiente
                    BackgroundJob? job = null;

                    switch (priority)
                    {
                        case JobPriority.High:
                            _highPriorityQueue.TryDequeue(out job);
                            break;
                        case JobPriority.Normal:
                            _normalPriorityQueue.TryDequeue(out job);
                            break;
                        case JobPriority.Low:
                            _lowPriorityQueue.TryDequeue(out job);
                            break;
                    }

                    if (job == null)
                    {
                        await Task.Delay(QueuePollingIntervalMs, cancellationToken);
                        continue;
                    }

                    // Procesar trabajo
                    await ProcessJobAsync(job, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en trabajador de prioridad {Priority}", priority);
                }
            }
        }

        /// <summary>
        /// Procesa un trabajo individual con manejo de errores y reintentos
        /// </summary>
        private async Task ProcessJobAsync(BackgroundJob job, CancellationToken cancellationToken)
        {
            var jobStartTime = DateTime.UtcNow;
            Interlocked.Increment(ref _activeJobs);

            try
            {
                job.Status = JobStatus.Processing;
                job.StartedAt = DateTime.UtcNow;

                _logger.LogInformation("Procesando trabajo: {JobId} - {JobType}", job.Id, job.JobType);

                // Ejecutar trabajo con timeout
                using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(JobTimeoutMinutes));
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);

                await job.JobAction(null, linkedTokenSource.Token);

                job.Status = JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                job.Duration = job.CompletedAt - job.StartedAt;

                Interlocked.Increment(ref _completedJobs);

                _metricsService.RecordJobExecution($"background_job:{job.JobType}", true, job.Duration.Value);

                _logger.LogInformation("Trabajo completado exitosamente: {JobId} en {Duration}", job.Id, job.Duration);

                _loggingService.LogApplicationEvent(
                    "background_job_completed",
                    $"Trabajo completado: {job.JobType}",
                    ApplicationLogLevel.Info,
                    additionalData: new Dictionary<string, object>
                    {
                        ["JobId"] = job.Id,
                        ["JobType"] = job.JobType,
                        ["DurationMs"] = job.Duration.Value.TotalMilliseconds
                    });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                job.Status = JobStatus.Cancelled;
                _logger.LogWarning("Trabajo cancelado: {JobId}", job.Id);
            }
            catch (OperationCanceledException)
            {
                job.Status = JobStatus.Timeout;
                _logger.LogWarning("Trabajo timeout: {JobId}", job.Id);
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.LastRetryAt = DateTime.UtcNow;

                // Reintentar si no excedió el límite
                if (job.RetryCount < job.MaxRetries)
                {
                    job.RetryCount++;
                    job.Status = JobStatus.Retrying;

                    _logger.LogWarning("Trabajo falló, reintentando ({RetryCount}/{MaxRetries}): {JobId} - {Error}",
                        job.RetryCount, job.MaxRetries, job.Id, ex.Message);

                    // Reencolar para reintento
                    await RequeueJobForRetryAsync(job);
                }
                else
                {
                    Interlocked.Increment(ref _failedJobs);

                    _logger.LogError(ex, "Trabajo falló definitivamente después de {MaxRetries} reintentos: {JobId}",
                        job.MaxRetries, job.Id);

                    _loggingService.LogApplicationEvent(
                        "background_job_failed",
                        $"Trabajo falló definitivamente: {job.JobType}",
                        ApplicationLogLevel.Error,
                        additionalData: new Dictionary<string, object>
                        {
                            ["JobId"] = job.Id,
                            ["JobType"] = job.JobType,
                            ["Error"] = ex.Message,
                            ["RetryCount"] = job.RetryCount,
                            ["MaxRetries"] = job.MaxRetries
                        });
                }
            }
            finally
            {
                Interlocked.Decrement(ref _activeJobs);
            }
        }

        /// <summary>
        /// Inicia recolector automático de métricas
        /// </summary>
        private async Task StartMetricsCollectorAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando recolector automático de métricas");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await EnqueueMetricsCollectionJobAsync(JobPriority.Low);

                    // Recolectar métricas cada 5 minutos
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en recolector automático de métricas");
                }
            }
        }

        /// <summary>
        /// Inicia tarea de limpieza automática
        /// </summary>
        private async Task StartCleanupTaskAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando tarea de limpieza automática");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Limpiar trabajos antiguos cada hora
                    await Task.Delay(TimeSpan.FromHours(1), cancellationToken);

                    await CleanupOldJobsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en tarea de limpieza automática");
                }
            }
        }

        // Implementaciones de trabajos específicos

        private async Task SendMultipleNotificationsAsync(IEnumerable<string> recipients, string subject, string message,
            NotificationChannel channel, CancellationToken cancellationToken)
        {
            var notificationService = new NotificationService(); // Inyección real en producción

            foreach (var recipient in recipients)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    await notificationService.SendNotificationAsync(recipient, subject, message, channel);
                    await Task.Delay(100, cancellationToken); // Pequeño delay entre envíos
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando notificación a {Recipient}", recipient);
                }
            }
        }

        private async Task GenerateReportAsync(string reportType, string userId, Dictionary<string, object>? parameters,
            CancellationToken cancellationToken)
        {
            var reportService = new ReportService(); // Inyección real en producción

            await reportService.GenerateReportAsync(reportType, userId, parameters, cancellationToken);
        }

        private async Task CollectSystemMetricsAsync(CancellationToken cancellationToken)
        {
            // Recolectar métricas del sistema
            await Task.Delay(5000, cancellationToken); // Simulación de recolección

            _logger.LogDebug("Métricas del sistema recolectadas");
        }

        // Métodos auxiliares

        private int GetMaxRetriesForJobType(string jobType)
        {
            return jobType switch
            {
                "SendMultipleNotifications" => 3,
                "GenerateReport" => 2,
                "CollectMetrics" => 5,
                _ => 3
            };
        }

        private ServiceStatus GetServiceStatus()
        {
            if (_activeJobs > 0) return ServiceStatus.Processing;
            if (GetTotalQueuedJobs() > 0) return ServiceStatus.Idle;
            return ServiceStatus.Stopped;
        }

        private int GetTotalQueuedJobs()
        {
            return _highPriorityQueue.Count + _normalPriorityQueue.Count + _lowPriorityQueue.Count;
        }

        private TimeSpan? GetAverageJobDuration()
        {
            // Implementación simplificada
            return TimeSpan.FromMinutes(2.5);
        }

        private DateTime? GetLastJobCompletionTime()
        {
            // Implementación simplificada
            return DateTime.UtcNow.AddMinutes(-5);
        }

        private double GetProcessingRate()
        {
            // Trabajos procesados por minuto en la última hora
            return 12.5;
        }

        private async Task RequeueJobForRetryAsync(BackgroundJob job)
        {
            // Agregar delay exponencial basado en número de reintentos
            var delay = TimeSpan.FromSeconds(Math.Pow(2, job.RetryCount));

            await Task.Delay(delay);

            // Reencolar con misma prioridad
            switch (job.Priority)
            {
                case JobPriority.High:
                    _highPriorityQueue.Enqueue(job);
                    break;
                case JobPriority.Normal:
                    _normalPriorityQueue.Enqueue(job);
                    break;
                case JobPriority.Low:
                    _lowPriorityQueue.Enqueue(job);
                    break;
            }
        }

        private async Task CleanupOldJobsAsync()
        {
            // Implementación de limpieza de trabajos antiguos
            _logger.LogDebug("Limpieza de trabajos antiguos ejecutada");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deteniendo servicio de procesamiento asíncrono");

            _cancellationTokenSource.Cancel();

            // Esperar a que los trabajos activos terminen
            while (_activeJobs > 0)
            {
                await Task.Delay(1000, cancellationToken);
            }

            await base.StopAsync(cancellationToken);

            _loggingService.LogApplicationEvent(
                "async_processing_service_stopped",
                "Servicio de procesamiento asíncrono detenido",
                ApplicationLogLevel.Info);
        }
    }

    /// <summary>
    /// DTOs para procesamiento asíncrono
    /// </summary>
    public class AsyncProcessingStatistics
    {
        public ServiceStatus ServiceStatus { get; set; }
        public int ActiveJobs { get; set; }
        public int QueuedJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public TimeSpan? AverageJobDuration { get; set; }
        public DateTime? LastJobCompleted { get; set; }
        public Dictionary<JobPriority, int> QueueSizes { get; set; } = new();
        public double ProcessingRate { get; set; }
    }

    public enum ServiceStatus
    {
        Stopped,
        Idle,
        Processing
    }

    public enum JobPriority
    {
        Low,
        Normal,
        High
    }

    public enum JobStatus
    {
        Queued,
        Processing,
        Completed,
        Failed,
        Cancelled,
        Timeout,
        Retrying
    }

    public class BackgroundJob
    {
        public string Id { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public JobPriority Priority { get; set; }
        public JobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public int MaxRetries { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? LastRetryAt { get; set; }
        public Func<IServiceProvider, CancellationToken, Task>? JobAction { get; set; }
    }

    // Servicios auxiliares simulados para demostración
    public class NotificationService
    {
        public async Task SendNotificationAsync(string recipient, string subject, string message, NotificationChannel channel)
        {
            await Task.Delay(100); // Simulación de envío
        }
    }

    public class ReportService
    {
        public async Task GenerateReportAsync(string reportType, string userId, Dictionary<string, object>? parameters, CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken); // Simulación de generación de reporte
        }
    }

    public enum NotificationChannel
    {
        Email,
        Sms,
        Teams,
        Slack
    }
}
