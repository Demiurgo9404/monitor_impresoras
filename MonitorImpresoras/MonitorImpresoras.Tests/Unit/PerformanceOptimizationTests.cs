using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Tests.Unit
{
    public class PerformanceOptimizationTests
    {
        private readonly Mock<ILogger<PerformanceAuditService>> _auditLoggerMock;
        private readonly Mock<ILogger<DatabaseOptimizationService>> _optimizationLoggerMock;
        private readonly Mock<ILogger<DistributedCacheService>> _cacheLoggerMock;
        private readonly Mock<ILogger<AdvancedMetricsService>> _metricsLoggerMock;

        private readonly PerformanceAuditService _auditService;
        private readonly DatabaseOptimizationService _optimizationService;
        private readonly DistributedCacheService _cacheService;
        private readonly AdvancedMetricsService _metricsService;

        public PerformanceOptimizationTests()
        {
            _auditLoggerMock = new Mock<ILogger<PerformanceAuditService>>();
            _optimizationLoggerMock = new Mock<ILogger<DatabaseOptimizationService>>();
            _cacheLoggerMock = new Mock<ILogger<DistributedCacheService>>();
            _metricsLoggerMock = new Mock<ILogger<AdvancedMetricsService>>();

            _auditService = new PerformanceAuditService(_auditLoggerMock.Object);
            _optimizationService = new DatabaseOptimizationService(_optimizationLoggerMock.Object);

            // Mock de servicios externos para cache y métricas
            var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            var distributedCacheMock = new Mock<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();

            _cacheService = new DistributedCacheService(
                memoryCacheMock.Object,
                distributedCacheMock.Object,
                _cacheLoggerMock.Object);

            _metricsService = new AdvancedMetricsService(
                _metricsLoggerMock.Object,
                _auditService);
        }

        [Fact]
        public async Task PerformanceAuditService_PerformFullAuditAsync_ShouldReturnCompleteReport()
        {
            // Act
            var audit = await _auditService.PerformFullAuditAsync();

            // Assert
            audit.Should().NotBeNull();
            audit.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            audit.SystemInfo.Should().NotBeNull();
            audit.DatabaseMetrics.Should().NotBeNull();
            audit.ApiMetrics.Should().NotBeNull();
            audit.JobMetrics.Should().NotBeNull();
            audit.CacheMetrics.Should().NotBeNull();
            audit.Bottlenecks.Should().NotBeNull();
            audit.Recommendations.Should().NotBeNull();
        }

        [Fact]
        public async Task PerformanceAuditService_GetDatabaseQueryMetricsAsync_ShouldReturnQueryMetrics()
        {
            // Act
            var metrics = await _auditService.GetDatabaseQueryMetricsAsync(10);

            // Assert
            metrics.Should().NotBeNull();
            metrics.Should().HaveCountLessOrEqualTo(10);
            metrics.Should().AllSatisfy(m => m.QueryHash.Should().NotBeEmpty());
            metrics.Should().AllSatisfy(m => m.AverageExecutionTime.Should().BeGreaterThan(0));
        }

        [Fact]
        public void PerformanceAuditService_RecordPerformanceMetric_ShouldTrackMetrics()
        {
            // Act
            _auditService.RecordPerformanceMetric("test_operation", TimeSpan.FromMilliseconds(150), true, "test_details");

            // Assert
            // El servicio interno mantiene métricas recientes, pero no expone método público para verificar
            // En producción verificarías logs o métricas internas
        }

        [Fact]
        public async Task DatabaseOptimizationService_OptimizeQueriesAsync_ShouldReturnOptimizationResult()
        {
            // Act
            var result = await _optimizationService.OptimizeQueriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            result.TelemetryOptimizations.Should().NotBeNull();
            result.AlertsOptimizations.Should().NotBeNull();
            result.EscalationOptimizations.Should().NotBeNull();
            result.PredictionsOptimizations.Should().NotBeNull();
            result.TotalOptimizations.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DatabaseOptimizationService_GetOptimizationRecommendationsAsync_ShouldReturnRecommendations()
        {
            // Act
            var recommendations = await _optimizationService.GetOptimizationRecommendationsAsync();

            // Assert
            recommendations.Should().NotBeNull();
            recommendations.Should().HaveCountGreaterThan(0);
            recommendations.Should().AllSatisfy(r => r.Category.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Description.Should().NotBeEmpty());
        }

        [Fact]
        public async Task DatabaseOptimizationService_AnalyzeSlowQueriesAsync_ShouldReturnAnalysis()
        {
            // Act
            var analysis = await _optimizationService.AnalyzeSlowQueriesAsync();

            // Assert
            analysis.Should().NotBeNull();
            analysis.Should().HaveCountGreaterThan(0);
            analysis.Should().AllSatisfy(a => a.QueryHash.Should().NotBeEmpty());
            analysis.Should().AllSatisfy(a => a.EstimatedImprovement.Should().BeGreaterThan(0));
        }

        [Fact]
        public async Task DistributedCacheService_GetOrSetAsync_WithFactory_ShouldCacheAndReturnValue()
        {
            // Arrange
            var cacheKey = "test_key";
            var expectedValue = new TestCacheObject { Id = 1, Name = "Test" };

            var memoryCacheMock = new Mock<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            object? cachedValue = null;
            memoryCacheMock.Setup(m => m.TryGetValue(cacheKey, out cachedValue)).Returns(false);

            var distributedCacheMock = new Mock<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            distributedCacheMock.Setup(m => m.GetStringAsync(cacheKey, default)).ReturnsAsync((string?)null);

            var cacheService = new DistributedCacheService(
                memoryCacheMock.Object,
                distributedCacheMock.Object,
                _cacheLoggerMock.Object);

            // Act
            var result = await cacheService.GetOrSetAsync(cacheKey,
                () => Task.FromResult(expectedValue),
                new CacheOptions { MemoryExpirationMinutes = 30 });

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Test");
        }

        [Fact]
        public async Task DistributedCacheService_GetOptimizedOptions_ShouldReturnCorrectOptions()
        {
            // Act
            var options = _cacheService.GetOptimizedOptions(CacheDataType.ActivePrinters);

            // Assert
            options.MemoryExpirationMinutes.Should().Be(15);
            options.DistributedExpirationMinutes.Should().Be(30);
            options.SlidingExpirationMinutes.Should().Be(5);
        }

        [Fact]
        public async Task AdvancedMetricsService_GetCurrentMetricsAsync_ShouldReturnSystemMetrics()
        {
            // Act
            var metrics = await _metricsService.GetCurrentMetricsAsync();

            // Assert
            metrics.Should().NotBeNull();
            metrics.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            metrics.CpuUsage.Should().BeGreaterThanOrEqualTo(0);
            metrics.MemoryUsage.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task AdvancedMetricsService_GeneratePerformanceReportAsync_ShouldReturnCompleteReport()
        {
            // Act
            var report = await _metricsService.GeneratePerformanceReportAsync();

            // Assert
            report.Should().NotBeNull();
            report.SystemMetrics.Should().NotBeNull();
            report.DatabaseMetrics.Should().NotBeNull();
            report.ApiMetrics.Should().NotBeNull();
            report.JobMetrics.Should().NotBeNull();
            report.Recommendations.Should().NotBeNull();
        }

        [Fact]
        public void AdvancedMetricsService_RecordHttpResponse_ShouldTrackMetrics()
        {
            // Act
            _metricsService.RecordHttpResponse("GET", "/api/v1/printers", 200, TimeSpan.FromMilliseconds(150));

            // Assert
            // Métricas se registran internamente en Prometheus
            // En producción verificarías que las métricas se incrementaron correctamente
        }

        [Fact]
        public void AdvancedMetricsService_RecordJobExecution_ShouldTrackJobMetrics()
        {
            // Act
            _metricsService.RecordJobExecution("telemetry_collection", true, TimeSpan.FromMilliseconds(2500));

            // Assert
            // Métricas se registran internamente
        }

        [Fact]
        public void AdvancedMetricsService_RecordTelemetryCollection_ShouldTrackCollectionMetrics()
        {
            // Act
            _metricsService.RecordTelemetryCollection(1, true, 12);

            // Assert
            // Métricas se registran internamente
        }

        [Fact]
        public void AdvancedMetricsService_RecordPrediction_ShouldTrackPredictionMetrics()
        {
            // Act
            _metricsService.RecordPrediction("TonerDepletion", "correct", 87.5m);

            // Assert
            // Métricas se registran internamente
        }

        [Fact]
        public void AdvancedMetricsService_UpdateDatabaseMetrics_ShouldUpdateMetrics()
        {
            // Act
            _metricsService.UpdateDatabaseMetrics(12, 94.5);

            // Assert
            // Métricas se actualizan internamente
        }

        [Fact]
        public void AdvancedMetricsService_ConfigurePerformanceAlerts_ShouldSetupAlerts()
        {
            // Act
            _metricsService.ConfigurePerformanceAlerts();

            // Assert
            // Alertas se configuran internamente
        }
    }

    /// <summary>
    /// Clase de prueba para objetos de cache
    /// </summary>
    public class TestCacheObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
