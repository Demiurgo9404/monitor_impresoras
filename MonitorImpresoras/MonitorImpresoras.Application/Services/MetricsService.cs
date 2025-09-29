using Prometheus;
using System.Diagnostics;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para métricas de la aplicación con Prometheus
    /// </summary>
    public class MetricsService : IMetricsService
    {
        private readonly Counter _totalRequests;
        private readonly Histogram _requestDuration;
        private readonly Counter _reportGenerations;
        private readonly Counter _reportGenerationErrors;
        private readonly Counter _emailSent;
        private readonly Counter _emailErrors;
        private readonly Gauge _activeUsers;
        private readonly Gauge _scheduledReportsActive;
        private readonly Counter _securityEvents;
        private readonly Counter _systemEvents;
        private readonly Histogram _databaseQueryDuration;

        public MetricsService()
        {
            _totalRequests = Metrics.CreateCounter("api_requests_total", "Total number of API requests",
                new CounterConfiguration { LabelNames = new[] { "method", "endpoint", "status_code" } });

            _requestDuration = Metrics.CreateHistogram("api_request_duration_seconds", "API request duration in seconds",
                new HistogramConfiguration { LabelNames = new[] { "method", "endpoint" }, Buckets = Histogram.LinearBuckets(0.1, 0.1, 10) });

            _reportGenerations = Metrics.CreateCounter("reports_generated_total", "Total number of reports generated",
                new CounterConfiguration { LabelNames = new[] { "format", "template", "status" } });

            _reportGenerationErrors = Metrics.CreateCounter("reports_generation_errors_total", "Total number of report generation errors",
                new CounterConfiguration { LabelNames = new[] { "format", "template", "error_type" } });

            _emailSent = Metrics.CreateCounter("emails_sent_total", "Total number of emails sent",
                new CounterConfiguration { LabelNames = new[] { "type", "status" } });

            _emailErrors = Metrics.CreateCounter("emails_errors_total", "Total number of email errors",
                new CounterConfiguration { LabelNames = new[] { "type", "error_type" } });

            _activeUsers = Metrics.CreateGauge("active_users", "Number of currently active users");

            _scheduledReportsActive = Metrics.CreateGauge("scheduled_reports_active", "Number of active scheduled reports");

            _securityEvents = Metrics.CreateCounter("security_events_total", "Total number of security events",
                new CounterConfiguration { LabelNames = new[] { "event_type", "severity" } });

            _systemEvents = Metrics.CreateCounter("system_events_total", "Total number of system events",
                new CounterConfiguration { LabelNames = new[] { "event_type", "category", "severity" } });

            _databaseQueryDuration = Metrics.CreateHistogram("database_query_duration_seconds", "Database query duration in seconds",
                new HistogramConfiguration { LabelNames = new[] { "operation", "table" }, Buckets = Histogram.LinearBuckets(0.01, 0.01, 10) });
        }

        public void RecordApiRequest(string method, string endpoint, int statusCode, double durationSeconds)
        {
            _totalRequests.Labels(method, endpoint, statusCode.ToString()).Inc();
            _requestDuration.Labels(method, endpoint).Observe(durationSeconds);
        }

        public void RecordReportGeneration(string format, string template, bool success, string? errorType = null)
        {
            var status = success ? "success" : "failed";
            _reportGenerations.Labels(format, template, status).Inc();

            if (!success && !string.IsNullOrEmpty(errorType))
            {
                _reportGenerationErrors.Labels(format, template, errorType).Inc();
            }
        }

        public void RecordEmailSent(string type, bool success, string? errorType = null)
        {
            var status = success ? "sent" : "failed";
            _emailSent.Labels(type, status).Inc();

            if (!success && !string.IsNullOrEmpty(errorType))
            {
                _emailErrors.Labels(type, errorType).Inc();
            }
        }

        public void SetActiveUsers(int count)
        {
            _activeUsers.Set(count);
        }

        public void SetActiveScheduledReports(int count)
        {
            _scheduledReportsActive.Set(count);
        }

        public void RecordSecurityEvent(string eventType, string severity)
        {
            _securityEvents.Labels(eventType, severity).Inc();
        }

        public void RecordSystemEvent(string eventType, string category, string severity)
        {
            _systemEvents.Labels(eventType, category, severity).Inc();
        }

        public IDisposable MeasureDatabaseQuery(string operation, string table)
        {
            return _databaseQueryDuration.Labels(operation, table).NewTimer();
        }

        public async Task UpdateActiveUsersAsync(IUserService userService)
        {
            try
            {
                // Obtener usuarios activos en las últimas 24 horas
                var activeUsers = await userService.GetActiveUsersCountAsync(24);
                SetActiveUsers(activeUsers);
            }
            catch (Exception ex)
            {
                // Log but don't throw - metrics shouldn't break the application
            }
        }

        public async Task UpdateScheduledReportsCountAsync(IScheduledReportService scheduledReportService)
        {
            try
            {
                var activeReports = await scheduledReportService.GetUserScheduledReportsAsync("system");
                SetActiveScheduledReports(activeReports.Count());
            }
            catch (Exception ex)
            {
                // Log but don't throw - metrics shouldn't break the application
            }
        }

        public string GetMetricsSnapshot()
        {
            return string.Empty; // Prometheus maneja esto automáticamente
        }
    }
}
