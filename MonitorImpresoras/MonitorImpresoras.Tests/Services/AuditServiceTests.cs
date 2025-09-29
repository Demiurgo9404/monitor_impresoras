using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Services;

namespace MonitorImpresoras.Tests.Services
{
    public class AuditServiceTests
    {
        private readonly Mock<ApplicationDbContext> _contextMock;
        private readonly Mock<ILogger<AuditService>> _loggerMock;
        private readonly AuditService _auditService;

        public AuditServiceTests()
        {
            _contextMock = new Mock<ApplicationDbContext>();
            _loggerMock = new Mock<ILogger<AuditService>>();

            _auditService = new AuditService(_contextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LogAsync_ShouldAddAuditLogToContext_WhenCalled()
        {
            // Arrange
            var userId = "test-user-id";
            var action = "TEST_ACTION";
            var entity = "TestEntity";
            var entityId = "test-entity-id";
            var details = "Test details";
            var ipAddress = "192.168.1.1";
            var userAgent = "Test User Agent";

            var auditLogsDbSetMock = new Mock<DbSet<AuditLog>>();
            _contextMock.Setup(c => c.AuditLogs).Returns(auditLogsDbSetMock.Object);

            // Act
            await _auditService.LogAsync(userId, action, entity, entityId, details, ipAddress, userAgent);

            // Assert
            auditLogsDbSetMock.Verify(db => db.Add(It.Is<AuditLog>(log =>
                log.UserId == userId &&
                log.Action == action &&
                log.Entity == entity &&
                log.EntityId == entityId &&
                log.Details == details &&
                log.IpAddress == ipAddress &&
                log.UserAgent == userAgent &&
                log.Timestamp <= DateTime.UtcNow)), Times.Once);

            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task LogAsync_ShouldHandleExceptions_WhenSaveChangesFails()
        {
            // Arrange
            var userId = "test-user-id";
            var action = "TEST_ACTION";
            var entity = "TestEntity";

            var auditLogsDbSetMock = new Mock<DbSet<AuditLog>>();
            _contextMock.Setup(c => c.AuditLogs).Returns(auditLogsDbSetMock.Object);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ThrowsAsync(new Exception("Database error"));

            // Act
            await _auditService.LogAsync(userId, action, entity);

            // Assert
            // Should not throw exception, but should log the error
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error al registrar auditoría"), Times.Once);
        }

        [Fact]
        public async Task GetLogsAsync_ShouldReturnFilteredLogs_WhenFiltersProvided()
        {
            // Arrange
            var userId = "test-user-id";
            var action = "TEST_ACTION";
            var entity = "TestEntity";
            var fromDate = DateTime.UtcNow.AddDays(-1);
            var toDate = DateTime.UtcNow;

            var auditLogs = new List<AuditLog>
            {
                new AuditLog { Id = 1, UserId = userId, Action = action, Entity = entity, Timestamp = DateTime.UtcNow },
                new AuditLog { Id = 2, UserId = "other-user", Action = "OTHER_ACTION", Entity = entity, Timestamp = DateTime.UtcNow },
                new AuditLog { Id = 3, UserId = userId, Action = action, Entity = "OtherEntity", Timestamp = DateTime.UtcNow }
            }.AsQueryable();

            var auditLogsDbSetMock = new Mock<DbSet<AuditLog>>();
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.Provider).Returns(auditLogs.Provider);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.Expression).Returns(auditLogs.Expression);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.ElementType).Returns(auditLogs.ElementType);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.GetEnumerator()).Returns(auditLogs.GetEnumerator());

            _contextMock.Setup(c => c.AuditLogs).Returns(auditLogsDbSetMock.Object);

            // Act
            var result = await _auditService.GetLogsAsync(userId, action, entity, fromDate, toDate);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
            result.First().UserId.Should().Be(userId);
            result.First().Action.Should().Be(action);
            result.First().Entity.Should().Be(entity);
        }

        [Fact]
        public async Task GetLogsAsync_ShouldReturnAllLogs_WhenNoFiltersProvided()
        {
            // Arrange
            var auditLogs = new List<AuditLog>
            {
                new AuditLog { Id = 1, UserId = "user1", Action = "ACTION1", Entity = "Entity1", Timestamp = DateTime.UtcNow },
                new AuditLog { Id = 2, UserId = "user2", Action = "ACTION2", Entity = "Entity2", Timestamp = DateTime.UtcNow },
                new AuditLog { Id = 3, UserId = "user1", Action = "ACTION1", Entity = "Entity1", Timestamp = DateTime.UtcNow }
            }.AsQueryable();

            var auditLogsDbSetMock = new Mock<DbSet<AuditLog>>();
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.Provider).Returns(auditLogs.Provider);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.Expression).Returns(auditLogs.Expression);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.ElementType).Returns(auditLogs.ElementType);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.GetEnumerator()).Returns(auditLogs.GetEnumerator());

            _contextMock.Setup(c => c.AuditLogs).Returns(auditLogsDbSetMock.Object);

            // Act
            var result = await _auditService.GetLogsAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetLogsAsync_ShouldOrderByTimestampDescending()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var auditLogs = new List<AuditLog>
            {
                new AuditLog { Id = 1, Timestamp = now.AddHours(-2) },
                new AuditLog { Id = 2, Timestamp = now.AddHours(-1) },
                new AuditLog { Id = 3, Timestamp = now }
            }.AsQueryable();

            var auditLogsDbSetMock = new Mock<DbSet<AuditLog>>();
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.Provider).Returns(auditLogs.Provider);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.Expression).Returns(auditLogs.Expression);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.ElementType).Returns(auditLogs.ElementType);
            auditLogsDbSetMock.As<IQueryable<AuditLog>>().Setup(m => m.GetEnumerator()).Returns(auditLogs.GetEnumerator());

            _contextMock.Setup(c => c.AuditLogs).Returns(auditLogsDbSetMock.Object);

            // Act
            var result = await _auditService.GetLogsAsync();

            // Assert
            result.Should().HaveCount(3);
            result.First().Id.Should().Be(3); // Most recent first
            result.Last().Id.Should().Be(1); // Oldest last
        }
    }
}
