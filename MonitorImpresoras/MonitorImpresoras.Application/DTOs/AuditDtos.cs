namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para evento del sistema
    /// </summary>
    public class SystemEventDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Severity { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string? EventData { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? SessionId { get; set; }
        public string? RequestId { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public int? HttpStatusCode { get; set; }
        public long? ExecutionTimeMs { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime TimestampUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de eventos del sistema
    /// </summary>
    public class SystemEventStatisticsDto
    {
        public int TotalEvents { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public Dictionary<string, int> EventsByCategory { get; set; } = new();
        public Dictionary<string, int> EventsBySeverity { get; set; } = new();
        public int SuccessfulEvents { get; set; }
        public int FailedEvents { get; set; }
        public double AverageExecutionTimeMs { get; set; }
        public Dictionary<string, int> EventsByUser { get; set; } = new();
        public Dictionary<string, int> MostCommonErrors { get; set; } = new();
    }

    /// <summary>
    /// DTO para filtro de eventos del sistema
    /// </summary>
    public class SystemEventFilterDto
    {
        public string? EventType { get; set; }
        public string? Category { get; set; }
        public string? Severity { get; set; }
        public string? UserId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// DTO para health check básico
    /// </summary>
    public class HealthCheckDto
    {
        public string Status { get; set; } = "Healthy";
        public Dictionary<string, object> Checks { get; set; } = new();
        public TimeSpan TotalDuration { get; set; }
    }

    /// <summary>
    /// DTO para health check extendido
    /// </summary>
    public class ExtendedHealthCheckDto : HealthCheckDto
    {
        public DatabaseHealthDto? Database { get; set; }
        public ScheduledReportsHealthDto? ScheduledReports { get; set; }
        public SystemHealthDto? System { get; set; }
        public MetricsHealthDto? Metrics { get; set; }
    }

    /// <summary>
    /// DTO para estado de base de datos
    /// </summary>
    public class DatabaseHealthDto
    {
        public string Status { get; set; } = "Healthy";
        public long ConnectionCount { get; set; }
        public Dictionary<string, long> TableRecordCounts { get; set; } = new();
        public TimeSpan QueryTime { get; set; }
        public string? ConnectionString { get; set; }
    }

    /// <summary>
    /// DTO para estado de reportes programados
    /// </summary>
    public class ScheduledReportsHealthDto
    {
        public string Status { get; set; } = "Healthy";
        public int ActiveScheduledReports { get; set; }
        public int PendingExecutions { get; set; }
        public int FailedExecutionsLast24h { get; set; }
        public DateTime? LastExecutionTime { get; set; }
    }

    /// <summary>
    /// DTO para estado del sistema
    /// </summary>
    public class SystemHealthDto
    {
        public string Status { get; set; } = "Healthy";
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public int ActiveConnections { get; set; }
        public string ApplicationVersion { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Production";
        public TimeSpan Uptime { get; set; }
    }

    /// <summary>
    /// DTO para métricas de salud
    /// </summary>
    public class MetricsHealthDto
    {
        public string Status { get; set; } = "Healthy";
        public long TotalRequests { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public double RequestsPerSecond { get; set; }
        public Dictionary<string, double> EndpointLatencies { get; set; } = new();
        public Dictionary<string, long> ErrorCounts { get; set; } = new();
    }
}
