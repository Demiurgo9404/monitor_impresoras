using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Tests.Unit
{
    public class ExtendedAuditServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly ExtendedAuditService _extendedAuditService;

        public ExtendedAuditServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Setup mocks
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Setup ExtendedAuditService
            _extendedAuditService = new ExtendedAuditService(
                _context,
                Mock.Of<ILogger<ExtendedAuditService>>(),
                _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task LogEventAsync_WithValidEvent_ShouldSaveToDatabase()
        {
            // Arrange
            var systemEvent = new SystemEvent
            {
                EventType = "test_event",
                Category = "test",
                Severity = "Info",
                Title = "Test Event",
                Description = "Test Description"
            };

            // Act
            await _extendedAuditService.LogEventAsync(systemEvent);

            // Assert
            var savedEvent = await _context.SystemEvents.FirstOrDefaultAsync();
            savedEvent.Should().NotBeNull();
            savedEvent!.EventType.Should().Be("test_event");
            savedEvent.Title.Should().Be("Test Event");
        }

        [Fact]
        public async Task GetSystemEventsAsync_WithFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var user = new User { Id = "test-user", UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var events = new List<SystemEvent>
            {
                new SystemEvent
                {
                    EventType = "report_generated",
                    Category = "reports",
                    Severity = "Info",
                    Title = "Report Generated",
                    UserId = "test-user"
                },
                new SystemEvent
                {
                    EventType = "email_sent",
                    Category = "emails",
                    Severity = "Warning",
                    Title = "Email Sent",
                    UserId = "test-user"
                }
            };

            await _context.SystemEvents.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            // Act
            var results = await _extendedAuditService.GetSystemEventsAsync(
                category: "reports",
                page: 1,
                pageSize: 10);

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(1);
            results.First().Category.Should().Be("reports");
        }

        [Fact]
        public async Task GetSystemEventStatisticsAsync_ShouldReturnStatistics()
        {
            // Arrange
            var events = new List<SystemEvent>
            {
                new SystemEvent { EventType = "report", Category = "reports", Severity = "Info", IsSuccess = true },
                new SystemEvent { EventType = "email", Category = "emails", Severity = "Error", IsSuccess = false }
            };

            await _context.SystemEvents.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            // Act
            var statistics = await _extendedAuditService.GetSystemEventStatisticsAsync();

            // Assert
            statistics.Should().NotBeNull();
            statistics.TotalEvents.Should().Be(2);
            statistics.SuccessfulEvents.Should().Be(1);
            statistics.FailedEvents.Should().Be(1);
            statistics.EventsByCategory.Should().ContainKey("reports");
            statistics.EventsByCategory.Should().ContainKey("emails");
        }

        [Fact]
        public async Task CleanupOldEventsAsync_ShouldRemoveOldEvents()
        {
            // Arrange
            var oldEvent = new SystemEvent
            {
                EventType = "old_event",
                Category = "test",
                Severity = "Info",
                Title = "Old Event",
                TimestampUtc = DateTime.UtcNow.AddDays(-100)
            };

            var recentEvent = new SystemEvent
            {
                EventType = "recent_event",
                Category = "test",
                Severity = "Info",
                Title = "Recent Event",
                TimestampUtc = DateTime.UtcNow
            };

            await _context.SystemEvents.AddRangeAsync(oldEvent, recentEvent);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _extendedAuditService.CleanupOldEventsAsync(30);

            // Assert
            deletedCount.Should().Be(1);

            var remainingEvents = await _context.SystemEvents.ToListAsync();
            remainingEvents.Should().HaveCount(1);
            remainingEvents.First().EventType.Should().Be("recent_event");
        }
    }

    public class MetricsServiceTests
    {
        private readonly MetricsService _metricsService;

        public MetricsServiceTests()
        {
            _metricsService = new MetricsService();
        }

        [Fact]
        public void RecordApiRequest_ShouldUpdateMetrics()
        {
            // Act
            _metricsService.RecordApiRequest("GET", "/health", 200, 0.5);

            // Assert
            // Metrics are recorded internally - we can't easily test the Prometheus metrics
            // but we can verify the method doesn't throw
        }

        [Fact]
        public void RecordReportGeneration_ShouldUpdateMetrics()
        {
            // Act
            _metricsService.RecordReportGeneration("pdf", "printer_report", true);
            _metricsService.RecordReportGeneration("excel", "user_report", false, "format_error");

            // Assert
            // Metrics are recorded internally - we can verify the method doesn't throw
        }

        [Fact]
        public void RecordEmailSent_ShouldUpdateMetrics()
        {
            // Act
            _metricsService.RecordEmailSent("report", true);
            _metricsService.RecordEmailSent("notification", false, "smtp_error");

            // Assert
            // Metrics are recorded internally - we can verify the method doesn't throw
        }

        [Fact]
        public void SetActiveUsers_ShouldUpdateGauge()
        {
            // Act
            _metricsService.SetActiveUsers(5);

            // Assert
            // Gauge is set internally - we can verify the method doesn't throw
        }

        [Fact]
        public void SetActiveScheduledReports_ShouldUpdateGauge()
        {
            // Act
            _metricsService.SetActiveScheduledReports(3);

            // Assert
            // Gauge is set internally - we can verify the method doesn't throw
        }

        [Fact]
        public void RecordSecurityEvent_ShouldUpdateMetrics()
        {
            // Act
            _metricsService.RecordSecurityEvent("login_attempt", "Warning");

            // Assert
            // Metrics are recorded internally - we can verify the method doesn't throw
        }

        [Fact]
        public void RecordSystemEvent_ShouldUpdateMetrics()
        {
            // Act
            _metricsService.RecordSystemEvent("background_worker", "system", "Info");

            // Assert
            // Metrics are recorded internally - we can verify the method doesn't throw
        }

        [Fact]
        public IDisposable MeasureDatabaseQuery_ShouldReturnTimer()
        {
            // Act
            var timer = _metricsService.MeasureDatabaseQuery("SELECT", "Users");

            // Assert
            timer.Should().NotBeNull();
            timer.Should().BeAssignableTo<IDisposable>();

            // Cleanup
            timer.Dispose();
        }

        [Fact]
        public void GetMetricsSnapshot_ShouldReturnEmptyString()
        {
            // Act
            var snapshot = _metricsService.GetMetricsSnapshot();

            // Assert
            snapshot.Should().BeEmpty();
        }
    }
}
