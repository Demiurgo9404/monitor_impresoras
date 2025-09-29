namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Tipos de severidad de notificaciones
    /// </summary>
    public enum NotificationSeverity
    {
        Critical,
        Warning,
        Info
    }

    /// <summary>
    /// Canales de notificación disponibles
    /// </summary>
    public enum NotificationChannel
    {
        Email,
        Slack,
        Teams,
        WhatsApp,
        Webhook
    }

    /// <summary>
    /// DTO para configuración de notificaciones
    /// </summary>
    public class NotificationConfigDto
    {
        public NotificationChannel Channel { get; set; }
        public bool Enabled { get; set; }
        public string? WebhookUrl { get; set; }
        public string? ApiKey { get; set; }
        public Dictionary<string, string>? CustomHeaders { get; set; }
    }

    /// <summary>
    /// DTO para envío de notificaciones
    /// </summary>
    public class NotificationRequestDto
    {
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public NotificationSeverity Severity { get; set; }
        public List<string> Recipients { get; set; } = new();
        public Dictionary<string, object>? Metadata { get; set; }
        public List<NotificationChannel> Channels { get; set; } = new();
        public bool RequireAcknowledgment { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de notificaciones
    /// </summary>
    public class NotificationResponseDto
    {
        public Guid NotificationId { get; set; }
        public DateTime SentAt { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RecipientsCount { get; set; }
    }

    /// <summary>
    /// DTO para historial de notificaciones
    /// </summary>
    public class NotificationHistoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public NotificationSeverity Severity { get; set; }
        public NotificationChannel Channel { get; set; }
        public DateTime SentAt { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Recipients { get; set; } = new();
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de notificaciones
    /// </summary>
    public class NotificationStatisticsDto
    {
        public int TotalSent { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public Dictionary<NotificationSeverity, int> BySeverity { get; set; } = new();
        public Dictionary<NotificationChannel, int> ByChannel { get; set; } = new();
        public Dictionary<string, int> ByRecipient { get; set; } = new();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
