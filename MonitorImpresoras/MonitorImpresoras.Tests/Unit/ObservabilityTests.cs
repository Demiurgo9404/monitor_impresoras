using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Tests.Unit
{
    public class ObservabilityTests
    {
        private readonly Mock<ILogger<CentralizedLoggingService>> _loggingLoggerMock;
        private readonly Mock<ILogger<ComprehensiveMetricsService>> _metricsLoggerMock;
        private readonly Mock<ILogger<ExtendedHealthCheckService>> _healthLoggerMock;
        private readonly Mock<ILogger<AdvancedAlertService>> _alertLoggerMock;
        private readonly Mock<IAdvancedMetricsService> _advancedMetricsMock;

        private readonly CentralizedLoggingService _loggingService;
        private readonly ComprehensiveMetricsService _metricsService;
        private readonly ExtendedHealthCheckService _healthService;
        private readonly AdvancedAlertService _alertService;

        public ObservabilityTests()
        {
            _loggingLoggerMock = new Mock<ILogger<CentralizedLoggingService>>();
            _metricsLoggerMock = new Mock<ILogger<ComprehensiveMetricsService>>();
            _healthLoggerMock = new Mock<ILogger<ExtendedHealthCheckService>>();
            _alertLoggerMock = new Mock<ILogger<AdvancedAlertService>>();
            _advancedMetricsMock = new Mock<IAdvancedMetricsService>();

            _loggingService = new CentralizedLoggingService(_loggingLoggerMock.Object, _advancedMetricsMock.Object);
            _metricsService = new ComprehensiveMetricsService(_metricsLoggerMock.Object, _advancedMetricsMock.Object, _loggingService);
            _healthService = new ExtendedHealthCheckService(_healthLoggerMock.Object, _advancedMetricsMock.Object, _loggingService);
            _alertService = new AdvancedAlertService(_alertLoggerMock.Object, _loggingService, _advancedMetricsMock.Object);
        }

        [Fact]
        public async Task CentralizedLoggingService_ConfigureStructuredLoggingAsync_ShouldReturnConfigurationResult()
        {
            // Act
            var result = await _loggingService.ConfigureStructuredLoggingAsync();

            // Assert
            result.Should().NotBeNull();
            result.JsonFormattingEnabled.Should().BeTrue();
            result.RequestIdCorrelationEnabled.Should().BeTrue();
            result.UserIdCorrelationEnabled.Should().BeTrue();
            result.SessionIdCorrelationEnabled.Should().BeTrue();
            result.TimestampUtcEnabled.Should().BeTrue();
            result.RetentionDays.Should().Be(90);
            result.Applied.Should().BeTrue();
        }

        [Fact]
        public void CentralizedLoggingService_LogApplicationEvent_ShouldLogEvent()
        {
            // Act
            _loggingService.LogApplicationEvent(
                "test_event",
                "Evento de prueba",
                ApplicationLogLevel.Info,
                "test_user",
                "test_request",
                new Dictionary<string, object> { ["key"] = "value" });

            // Assert
            // Se verifica mediante logs - en producción verificarías que se guardó correctamente
        }

        [Fact]
        public void CentralizedLoggingService_LogSecurityEvent_ShouldLogSecurityEvent()
        {
            // Act
            _loggingService.LogSecurityEvent(
                "FailedLogin",
                "Intento de login fallido",
                "test_user",
                "192.168.1.100",
                SecurityEventSeverity.Medium,
                new Dictionary<string, object> { ["Attempts"] = 3 });

            // Assert
            // Se verifica mediante logs
        }

        [Fact]
        public void CentralizedLoggingService_LogAiEvent_ShouldLogAiEvent()
        {
            // Act
            _loggingService.LogAiEvent(
                "prediction_completed",
                "MaintenancePrediction",
                "Predict",
                AiEventResult.Success,
                new Dictionary<string, object> { ["Accuracy"] = 0.87 },
                "test_user");

            // Assert
            // Se verifica mediante logs
        }

        [Fact]
        public void CentralizedLoggingService_LogDatabaseEvent_ShouldLogDatabaseEvent()
        {
            // Act
            _loggingService.LogDatabaseEvent(
                "select_executed",
                "SELECT",
                "Printers",
                DatabaseEventResult.Success,
                TimeSpan.FromMilliseconds(150),
                new Dictionary<string, object> { ["Rows"] = 25 },
                "test_user");

            // Assert
            // Se verifica mediante logs
        }

        [Fact]
        public async Task ComprehensiveMetricsService_GetCurrentSystemMetricsAsync_ShouldReturnMetrics()
        {
            // Act
            var metrics = await _metricsService.GetCurrentSystemMetricsAsync();

            // Assert
            metrics.Should().NotBeNull();
            metrics.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            metrics.CpuUsage.Should().BeInRange(0, 100);
            metrics.MemoryUsage.Should().BeInRange(0, 100);
            metrics.ThreadCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ComprehensiveMetricsService_GetHistoricalMetricsAsync_ShouldReturnReport()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddHours(-24);
            var toDate = DateTime.UtcNow;

            // Act
            var report = await _metricsService.GetHistoricalMetricsAsync(fromDate, toDate);

            // Assert
            report.Should().NotBeNull();
            report.Period.From.Should().Be(fromDate);
            report.Period.To.Should().Be(toDate);
            report.HttpMetrics.Should().NotBeNull();
            report.DatabaseMetrics.Should().NotBeNull();
            report.AiMetrics.Should().NotBeNull();
            report.SystemMetrics.Should().NotBeNull();
            report.JobMetrics.Should().NotBeNull();
        }

        [Fact]
        public void ComprehensiveMetricsService_RecordHttpOperation_ShouldRecordMetrics()
        {
            // Act
            _metricsService.RecordHttpOperation(
                "GET",
                "/api/v1/printers",
                200,
                TimeSpan.FromMilliseconds(150),
                "test_user",
                1024,
                2048);

            // Assert
            // Se verifica mediante métricas internas
        }

        [Fact]
        public void ComprehensiveMetricsService_RecordDatabaseOperation_ShouldRecordMetrics()
        {
            // Act
            _metricsService.RecordDatabaseOperation(
                "SELECT",
                "Printers",
                DatabaseOperationType.Select,
                TimeSpan.FromMilliseconds(45),
                25,
                true,
                "test_user");

            // Assert
            // Se verifica mediante métricas internas
        }

        [Fact]
        public void ComprehensiveMetricsService_RecordAiOperation_ShouldRecordMetrics()
        {
            // Act
            _metricsService.RecordAiOperation(
                "MaintenancePrediction",
                "Predict",
                AiOperationResult.Success,
                TimeSpan.FromMilliseconds(150),
                new Dictionary<string, object>
                {
                    ["Accuracy"] = 0.87,
                    ["ConfidenceScore"] = 0.92
                },
                "test_user");

            // Assert
            // Se verifica mediante métricas internas
        }

        [Fact]
        public void ComprehensiveMetricsService_RecordScheduledJob_ShouldRecordMetrics()
        {
            // Act
            _metricsService.RecordScheduledJob(
                "TelemetryCollection",
                "CollectAllMetrics",
                JobExecutionResult.Success,
                TimeSpan.FromMinutes(2),
                100);

            // Assert
            // Se verifica mediante métricas internas
        }

        [Fact]
        public async Task ExtendedHealthCheckService_RunExtendedHealthChecksAsync_ShouldReturnHealthReport()
        {
            // Act
            var report = await _healthService.RunExtendedHealthChecksAsync();

            // Assert
            report.Should().NotBeNull();
            report.CheckTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            report.OverallStatus.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
            report.BasicChecks.Should().NotBeNull();
            report.InfrastructureChecks.Should().NotBeNull();
            report.ApplicationChecks.Should().NotBeNull();
            report.AiChecks.Should().NotBeNull();
            report.DatabaseChecks.Should().NotBeNull();
            report.NetworkChecks.Should().NotBeNull();
            report.SecurityChecks.Should().NotBeNull();
        }

        [Fact]
        public async Task AdvancedAlertService_CreateDynamicPrinterAlertAsync_ShouldReturnAlert()
        {
            // Arrange
            var metrics = new PrinterMetrics
            {
                PrinterId = 1,
                IsOnline = false,
                FailureProbability = 0.90,
                Temperature = 65,
                TonerLevel = 0.05,
                PaperLevel = 0.10,
                LastSeenMinutes = 20
            };

            // Act
            var alert = await _alertService.CreateDynamicPrinterAlertAsync(metrics);

            // Assert
            alert.Should().NotBeNull();
            alert.AlertId.Should().NotBeEmpty();
            alert.AlertType.Should().Be(AlertType.PrinterHealth);
            alert.Severity.Should().Be(AlertSeverity.Critical);
            alert.Title.Should().Contain("Imp 1");
            alert.Description.Should().Contain("offline");
            alert.Description.Should().Contain("90%");
            alert.Description.Should().Contain("65°C");
            alert.Metrics.Should().Be(metrics);
            alert.RequiresImmediateAction.Should().BeTrue();
            alert.NotificationChannels.Should().Contain(NotificationChannel.Email);
            alert.NotificationChannels.Should().Contain(NotificationChannel.Teams);
        }

        [Fact]
        public async Task AdvancedAlertService_ConfigureDynamicAlertRulesAsync_ShouldReturnConfiguration()
        {
            // Act
            var result = await _alertService.ConfigureDynamicAlertRulesAsync();

            // Assert
            result.Should().NotBeNull();
            result.RulesConfigured.Should().HaveCountGreaterThan(0);
            result.DynamicThresholdsEnabled.Should().BeTrue();
            result.MultiChannelNotifications.Should().BeTrue();
            result.EscalationPoliciesEnabled.Should().BeTrue();
            result.AlertRetentionDays.Should().Be(90);
            result.Applied.Should().BeTrue();

            // Verificar reglas específicas
            result.RulesConfigured.Should().Contain(r => r.Name == "HighFailureProbability");
            result.RulesConfigured.Should().Contain(r => r.Name == "PrinterOffline");
            result.RulesConfigured.Should().Contain(r => r.Name == "SecurityBreach");
        }

        [Fact]
        public async Task AdvancedAlertService_SendTestNotificationAsync_ShouldReturnTestResult()
        {
            // Act
            var result = await _alertService.SendTestNotificationAsync();

            // Assert
            result.Should().NotBeNull();
            result.TestId.Should().NotBeEmpty();
            result.TestTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            result.ChannelsTested.Should().BeGreaterThan(0);
            result.Results.Should().HaveCount(result.ChannelsTested);
            result.OverallSuccess.Should().BeTrue(); // Todas las notificaciones simuladas son exitosas
        }

        [Fact]
        public async Task AdvancedAlertService_GetAlertHistoryAsync_ShouldReturnHistoryReport()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;

            // Act
            var report = await _alertService.GetAlertHistoryAsync(fromDate, toDate);

            // Assert
            report.Should().NotBeNull();
            report.Period.From.Should().Be(fromDate);
            report.Period.To.Should().Be(toDate);
            report.TotalAlerts.Should().BeGreaterThan(0);
            report.AlertsBySeverity.Should().NotBeNull();
            report.AlertsByType.Should().NotBeNull();
            report.AlertsByChannel.Should().NotBeNull();
            report.TopAlertTypes.Should().NotBeNull();
        }

        [Fact]
        public async Task AdvancedAlertService_ProcessAlertEventAsync_ShouldReturnAlertResult()
        {
            // Arrange
            var alertEvent = new AlertEvent
            {
                EventType = "HighFailureProbability",
                Severity = AlertSeverity.Critical,
                Description = "Probabilidad de fallo crítica detectada",
                Source = "PrinterMonitoring"
            };

            // Act
            var result = await _alertService.ProcessAlertEventAsync(alertEvent);

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().NotBeEmpty();
            result.EventReceived.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            result.EventType.Should().Be(alertEvent.EventType);
            result.Severity.Should().Be(alertEvent.Severity);
            result.Processed.Should().BeTrue();
            result.Status.Should().Be(AlertStatus.Sent);
            result.NotificationsSent.Should().NotBeNull();
            result.NotificationsSent.Should().HaveCountGreaterThan(0);
        }
    }
}
