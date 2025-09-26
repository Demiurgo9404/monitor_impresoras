namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// DTO para información de tenant
    /// </summary>
    public class TenantInfo
    {
        public Guid Id { get; set; }
        public string TenantKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public SubscriptionTier Tier { get; set; }
    }

    /// <summary>
    /// DTO para snapshot de datos de reporte
    /// </summary>
    public class ReportDataSnapshot
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string DataJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resultado de análisis de alertas
    /// </summary>
    public class AlertAnalysisResult
    {
        public int Id { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para payload de notificación
    /// </summary>
    public class NotificationPayload
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contexto de tenant para operaciones multi-tenant
    /// </summary>
    public class TenantContext
    {
        public TenantInfo Tenant { get; set; } = new();
        public object Db { get; set; } = new();
    }
}
