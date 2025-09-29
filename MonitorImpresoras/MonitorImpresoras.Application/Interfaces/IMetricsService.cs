namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de métricas de la aplicación
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// Registra una solicitud API
        /// </summary>
        void RecordApiRequest(string method, string endpoint, int statusCode, double durationSeconds);

        /// <summary>
        /// Registra la generación de un reporte
        /// </summary>
        void RecordReportGeneration(string format, string template, bool success, string? errorType = null);

        /// <summary>
        /// Registra el envío de un email
        /// </summary>
        void RecordEmailSent(string type, bool success, string? errorType = null);

        /// <summary>
        /// Establece el número de usuarios activos
        /// </summary>
        void SetActiveUsers(int count);

        /// <summary>
        /// Establece el número de reportes programados activos
        /// </summary>
        void SetActiveScheduledReports(int count);

        /// <summary>
        /// Registra un evento de seguridad
        /// </summary>
        void RecordSecurityEvent(string eventType, string severity);

        /// <summary>
        /// Registra un evento del sistema
        /// </summary>
        void RecordSystemEvent(string eventType, string category, string severity);

        /// <summary>
        /// Mide la duración de una consulta a base de datos
        /// </summary>
        IDisposable MeasureDatabaseQuery(string operation, string table);

        /// <summary>
        /// Actualiza métricas de usuarios activos
        /// </summary>
        Task UpdateActiveUsersAsync(IUserService userService);

        /// <summary>
        /// Actualiza métricas de reportes programados activos
        /// </summary>
        Task UpdateScheduledReportsCountAsync(IScheduledReportService scheduledReportService);

        /// <summary>
        /// Obtiene snapshot de métricas (para exposición)
        /// </summary>
        string GetMetricsSnapshot();
    }
}
