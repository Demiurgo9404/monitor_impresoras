using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio completo de métricas de rendimiento para observabilidad
    /// </summary>
    public class ComprehensiveMetricsService : IComprehensiveMetricsService
    {
        private readonly ILogger<ComprehensiveMetricsService> _logger;
        private readonly IAdvancedMetricsService _advancedMetrics;
        private readonly ICentralizedLoggingService _loggingService;
        private readonly ConcurrentDictionary<string, PerformanceCounter> _performanceCounters = new();

        public ComprehensiveMetricsService(
            ILogger<ComprehensiveMetricsService> logger,
            IAdvancedMetricsService advancedMetrics,
            ICentralizedLoggingService loggingService)
        {
            _logger = logger;
            _advancedMetrics = advancedMetrics;
            _loggingService = loggingService;

            InitializePerformanceCounters();
        }

        /// <summary>
        /// Registra métricas completas de una operación HTTP
        /// </summary>
        public void RecordHttpOperation(string method, string endpoint, int statusCode, TimeSpan duration, string? userId = null, long? requestSize = null, long? responseSize = null)
        {
            try
            {
                // Métricas básicas HTTP
                _advancedMetrics.RecordHttpResponse(method, endpoint, statusCode, duration);

                // Métricas adicionales de rendimiento
                var metrics = new HttpOperationMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    Method = method,
                    Endpoint = endpoint,
                    StatusCode = statusCode,
                    Duration = duration,
                    UserId = userId,
                    RequestSize = requestSize ?? 0,
                    ResponseSize = responseSize ?? 0,
                    CpuUsage = GetCurrentCpuUsage(),
                    MemoryUsage = GetCurrentMemoryUsage(),
                    ThreadCount = GetCurrentThreadCount(),
                    GcCollections = GetGcCollectionsCount()
                };

                // Log estructurado si es operación lenta
                if (duration.TotalMilliseconds > 1000)
                {
                    _loggingService.LogApplicationEvent(
                        "slow_http_operation",
                        $"Operación HTTP lenta detectada: {method} {endpoint}",
                        ApplicationLogLevel.Warning,
                        userId,
                        additionalData: new Dictionary<string, object>
                        {
                            ["Duration"] = duration.TotalMilliseconds,
                            ["StatusCode"] = statusCode,
                            ["Endpoint"] = endpoint,
                            ["Method"] = method
                        });
                }

                _logger.LogDebug("Métricas HTTP registradas: {Method} {Endpoint} - {Duration}ms", method, endpoint, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métricas HTTP");
            }
        }

        /// <summary>
        /// Registra métricas completas de operación de base de datos
        /// </summary>
        public void RecordDatabaseOperation(string operation, string tableName, DatabaseOperationType type, TimeSpan duration, int rowsAffected, bool success, string? userId = null)
        {
            try
            {
                var metrics = new DatabaseOperationMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    Operation = operation,
                    TableName = tableName,
                    OperationType = type,
                    Duration = duration,
                    RowsAffected = rowsAffected,
                    Success = success,
                    UserId = userId,
                    ConnectionCount = GetActiveDatabaseConnections(),
                    QueryComplexity = CalculateQueryComplexity(operation)
                };

                // Log estructurado para operaciones lentas o fallidas
                if (duration.TotalMilliseconds > 2000 || !success)
                {
                    _loggingService.LogDatabaseEvent(
                        success ? "slow_database_operation" : "failed_database_operation",
                        operation,
                        tableName,
                        success ? DatabaseEventResult.Warning : DatabaseEventResult.Error,
                        duration,
                        new Dictionary<string, object>
                        {
                            ["RowsAffected"] = rowsAffected,
                            ["OperationType"] = type.ToString()
                        },
                        userId);
                }

                _logger.LogDebug("Métricas BD registradas: {Operation} en {TableName} - {Duration}ms", operation, tableName, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métricas de base de datos");
            }
        }

        /// <summary>
        /// Registra métricas completas de operación de IA
        /// </summary>
        public void RecordAiOperation(string modelType, string operation, AiOperationResult result, TimeSpan duration, Dictionary<string, object>? metrics = null, string? userId = null)
        {
            try
            {
                var aiMetrics = new AiOperationMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    ModelType = modelType,
                    Operation = operation,
                    Result = result,
                    Duration = duration,
                    UserId = userId,
                    CpuUsage = GetCurrentCpuUsage(),
                    MemoryUsage = GetCurrentMemoryUsage(),
                    ModelAccuracy = metrics?.GetValueOrDefault("Accuracy", 0.0) ?? 0.0,
                    PredictionsCount = metrics?.GetValueOrDefault("PredictionsCount", 0) ?? 0,
                    ConfidenceScore = metrics?.GetValueOrDefault("ConfidenceScore", 0.0) ?? 0.0
                };

                // Log estructurado para operaciones de IA
                _loggingService.LogAiEvent(
                    result == AiOperationResult.Success ? "ai_operation_success" : "ai_operation_failed",
                    modelType,
                    operation,
                    result == AiOperationResult.Success ? AiEventResult.Success : AiEventResult.Error,
                    metrics,
                    userId);

                // Métricas adicionales si es predicción
                if (operation.Contains("prediction") || operation.Contains("predict"))
                {
                    _advancedMetrics.RecordPrediction(modelType, result.ToString().ToLower(), Convert.ToDecimal(aiMetrics.ConfidenceScore));
                }

                _logger.LogDebug("Métricas IA registradas: {ModelType} {Operation} - {Duration}ms", modelType, operation, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métricas de IA");
            }
        }

        /// <summary>
        /// Registra métricas de sistema en tiempo real
        /// </summary>
        public async Task<SystemMetricsSnapshot> GetCurrentSystemMetricsAsync()
        {
            try
            {
                var process = Process.GetCurrentProcess();

                var snapshot = new SystemMetricsSnapshot
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsage = GetCurrentCpuUsage(),
                    MemoryUsage = process.WorkingSet64 / (double)GetTotalMemoryBytes() * 100,
                    AvailableMemory = GetAvailableMemoryBytes(),
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    GcHeapSize = GC.GetTotalMemory(false),
                    GcCollections0 = GC.CollectionCount(0),
                    GcCollections1 = GC.CollectionCount(1),
                    GcCollections2 = GC.CollectionCount(2),
                    WorkingSet = process.WorkingSet64,
                    PrivateMemorySize = process.PrivateMemorySize64,
                    VirtualMemorySize = process.VirtualMemorySize64,
                    DiskUsage = GetCurrentDiskUsage(),
                    NetworkBytesReceived = GetNetworkBytesReceived(),
                    NetworkBytesSent = GetNetworkBytesSent()
                };

                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas actuales del sistema");
                return new SystemMetricsSnapshot();
            }
        }

        /// <summary>
        /// Registra métricas de jobs programados
        /// </summary>
        public void RecordScheduledJob(string jobType, string jobName, JobExecutionResult result, TimeSpan duration, int itemsProcessed = 0)
        {
            try
            {
                var metrics = new ScheduledJobMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    JobType = jobType,
                    JobName = jobName,
                    Result = result,
                    Duration = duration,
                    ItemsProcessed = itemsProcessed,
                    CpuUsage = GetCurrentCpuUsage(),
                    MemoryUsage = GetCurrentMemoryUsage(),
                    SuccessRate = result == JobExecutionResult.Success ? 1.0 : 0.0
                };

                // Registrar en métricas avanzadas
                _advancedMetrics.RecordJobExecution(jobType, result == JobExecutionResult.Success, duration);

                // Log estructurado
                _loggingService.LogApplicationEvent(
                    result == JobExecutionResult.Success ? "scheduled_job_success" : "scheduled_job_failed",
                    $"Job {jobName} completado con resultado {result}",
                    result == JobExecutionResult.Success ? ApplicationLogLevel.Info : ApplicationLogLevel.Error,
                    additionalData: new Dictionary<string, object>
                    {
                        ["JobType"] = jobType,
                        ["Duration"] = duration.TotalMilliseconds,
                        ["ItemsProcessed"] = itemsProcessed
                    });

                _logger.LogDebug("Métricas de job registradas: {JobName} - {Result} en {Duration}ms", jobName, result, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando métricas de job programado");
            }
        }

        /// <summary>
        /// Obtiene métricas históricas para análisis
        /// </summary>
        public async Task<HistoricalMetricsReport> GetHistoricalMetricsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var report = new HistoricalMetricsReport
                {
                    Period = new DateTimeRange(fromDate, toDate),
                    HttpMetrics = await GetHistoricalHttpMetricsAsync(fromDate, toDate),
                    DatabaseMetrics = await GetHistoricalDatabaseMetricsAsync(fromDate, toDate),
                    AiMetrics = await GetHistoricalAiMetricsAsync(fromDate, toDate),
                    SystemMetrics = await GetHistoricalSystemMetricsAsync(fromDate, toDate),
                    JobMetrics = await GetHistoricalJobMetricsAsync(fromDate, toDate)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de métricas históricas");
                return new HistoricalMetricsReport();
            }
        }

        /// <summary>
        /// Inicializa contadores de rendimiento del sistema
        /// </summary>
        private void InitializePerformanceCounters()
        {
            try
            {
                // Inicializar contadores básicos del sistema
                _performanceCounters["Processor"] = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _performanceCounters["Memory"] = new PerformanceCounter("Memory", "Available MBytes");
                _performanceCounters["Network"] = new PerformanceCounter("Network Interface", "Bytes Received/sec", "Intel[R] Ethernet Connection");
                _performanceCounters["Disk"] = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

                _logger.LogInformation("Contadores de rendimiento inicializados");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron inicializar algunos contadores de rendimiento (modo desarrollo)");
            }
        }

        // Métodos auxiliares para obtener métricas del sistema
        private double GetCurrentCpuUsage() => 25.0; // Simulado
        private double GetCurrentMemoryUsage() => 45.0; // Simulado
        private int GetCurrentThreadCount() => 12; // Simulado
        private long GetGcCollectionsCount() => GC.CollectionCount(2); // Real
        private int GetActiveDatabaseConnections() => 8; // Simulado
        private long GetAvailableMemoryBytes() => 4L * 1024 * 1024 * 1024; // 4GB simulado
        private double GetCurrentDiskUsage() => 15.0; // Simulado
        private long GetNetworkBytesReceived() => 1024 * 1024; // 1MB simulado
        private long GetNetworkBytesSent() => 512 * 1024; // 512KB simulado
        private static long GetTotalMemoryBytes() => 8L * 1024 * 1024 * 1024; // 8GB simulado
        private QueryComplexity CalculateQueryComplexity(string query) => QueryComplexity.Simple; // Simulado

        // Métodos para obtener métricas históricas (simulados)
        private async Task<List<HttpOperationMetrics>> GetHistoricalHttpMetricsAsync(DateTime fromDate, DateTime toDate) =>
            new List<HttpOperationMetrics> { new() { Timestamp = DateTime.UtcNow, Method = "GET", Endpoint = "/api/v1/printers", Duration = TimeSpan.FromMilliseconds(150) } };

        private async Task<List<DatabaseOperationMetrics>> GetHistoricalDatabaseMetricsAsync(DateTime fromDate, DateTime toDate) =>
            new List<DatabaseOperationMetrics> { new() { Timestamp = DateTime.UtcNow, Operation = "SELECT", TableName = "Printers", Duration = TimeSpan.FromMilliseconds(45) } };

        private async Task<List<AiOperationMetrics>> GetHistoricalAiMetricsAsync(DateTime fromDate, DateTime toDate) =>
            new List<AiOperationMetrics> { new() { Timestamp = DateTime.UtcNow, ModelType = "MaintenancePrediction", Operation = "Predict", Duration = TimeSpan.FromMilliseconds(150) } };

        private async Task<List<SystemMetricsSnapshot>> GetHistoricalSystemMetricsAsync(DateTime fromDate, DateTime toDate) =>
            new List<SystemMetricsSnapshot> { await GetCurrentSystemMetricsAsync() };

        private async Task<List<ScheduledJobMetrics>> GetHistoricalJobMetricsAsync(DateTime fromDate, DateTime toDate) =>
            new List<ScheduledJobMetrics> { new() { Timestamp = DateTime.UtcNow, JobType = "TelemetryCollection", JobName = "CollectAllMetrics", Duration = TimeSpan.FromMinutes(2) } };
    }

    /// <summary>
    /// DTO para métricas de operación HTTP
    /// </summary>
    public class HttpOperationMetrics
    {
        public DateTime Timestamp { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public TimeSpan Duration { get; set; }
        public string? UserId { get; set; }
        public long RequestSize { get; set; }
        public long ResponseSize { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public int ThreadCount { get; set; }
        public int GcCollections { get; set; }
    }

    /// <summary>
    /// DTO para métricas de operación de base de datos
    /// </summary>
    public class DatabaseOperationMetrics
    {
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public DatabaseOperationType OperationType { get; set; }
        public TimeSpan Duration { get; set; }
        public int RowsAffected { get; set; }
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public int ConnectionCount { get; set; }
        public QueryComplexity QueryComplexity { get; set; }
    }

    /// <summary>
    /// DTO para métricas de operación de IA
    /// </summary>
    public class AiOperationMetrics
    {
        public DateTime Timestamp { get; set; }
        public string ModelType { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public AiOperationResult Result { get; set; }
        public TimeSpan Duration { get; set; }
        public string? UserId { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double ModelAccuracy { get; set; }
        public int PredictionsCount { get; set; }
        public double ConfidenceScore { get; set; }
    }

    /// <summary>
    /// DTO para snapshot de métricas del sistema
    /// </summary>
    public class SystemMetricsSnapshot
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public long AvailableMemory { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public long GcHeapSize { get; set; }
        public int GcCollections0 { get; set; }
        public int GcCollections1 { get; set; }
        public int GcCollections2 { get; set; }
        public long WorkingSet { get; set; }
        public long PrivateMemorySize { get; set; }
        public long VirtualMemorySize { get; set; }
        public double DiskUsage { get; set; }
        public long NetworkBytesReceived { get; set; }
        public long NetworkBytesSent { get; set; }
    }

    /// <summary>
    /// DTO para métricas de job programado
    /// </summary>
    public class ScheduledJobMetrics
    {
        public DateTime Timestamp { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public JobExecutionResult Result { get; set; }
        public TimeSpan Duration { get; set; }
        public int ItemsProcessed { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// DTO para reporte de métricas históricas
    /// </summary>
    public class HistoricalMetricsReport
    {
        public DateTimeRange Period { get; set; } = new();
        public List<HttpOperationMetrics> HttpMetrics { get; set; } = new();
        public List<DatabaseOperationMetrics> DatabaseMetrics { get; set; } = new();
        public List<AiOperationMetrics> AiMetrics { get; set; } = new();
        public List<SystemMetricsSnapshot> SystemMetrics { get; set; } = new();
        public List<ScheduledJobMetrics> JobMetrics { get; set; } = new();
    }

    /// <summary>
    /// Tipos de operación de base de datos
    /// </summary>
    public enum DatabaseOperationType
    {
        Select,
        Insert,
        Update,
        Delete,
        Create,
        Drop,
        Alter,
        BulkInsert,
        Transaction
    }

    /// <summary>
    /// Resultado de operación de IA
    /// </summary>
    public enum AiOperationResult
    {
        Success,
        PartialSuccess,
        Failed,
        Timeout,
        InvalidInput
    }

    /// <summary>
    /// Resultado de ejecución de job
    /// </summary>
    public enum JobExecutionResult
    {
        Success,
        PartialSuccess,
        Failed,
        Cancelled,
        Timeout
    }

    /// <summary>
    /// Complejidad de consulta
    /// </summary>
    public enum QueryComplexity
    {
        Simple,
        Medium,
        Complex,
        VeryComplex
    }
}
