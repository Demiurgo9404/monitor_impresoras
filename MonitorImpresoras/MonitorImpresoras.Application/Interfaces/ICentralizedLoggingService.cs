using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de logging centralizado estructurado
    /// </summary>
    public interface ICentralizedLoggingService
    {
        /// <summary>
        /// Registra evento estructurado de aplicación
        /// </summary>
        void LogApplicationEvent(string eventType, string message, ApplicationLogLevel level, string? userId = null, string? requestId = null, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Registra evento de seguridad estructurado
        /// </summary>
        void LogSecurityEvent(string eventType, string description, string userId, string ipAddress, SecurityEventSeverity severity, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Registra evento de IA/Machine Learning estructurado
        /// </summary>
        void LogAiEvent(string eventType, string modelType, string operation, AiEventResult result, Dictionary<string, object>? metrics = null, string? userId = null);

        /// <summary>
        /// Registra evento de base de datos estructurado
        /// </summary>
        void LogDatabaseEvent(string eventType, string operation, string tableName, DatabaseEventResult result, TimeSpan duration, Dictionary<string, object>? parameters = null, string? userId = null);

        /// <summary>
        /// Configura logging estructurado para toda la aplicación
        /// </summary>
        Task<LoggingConfigurationResult> ConfigureStructuredLoggingAsync();
    }
}
