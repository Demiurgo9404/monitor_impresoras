using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Tests.Unit
{
    public class ReportServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IReportRepository> _reportRepositoryMock;
        private readonly Mock<IPermissionService> _permissionServiceMock;
        private readonly ReportService _reportService;

        public ReportServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Setup mocks
            _reportRepositoryMock = new Mock<IReportRepository>();
            _permissionServiceMock = new Mock<IPermissionService>();

            // Setup ReportService
            _reportService = new ReportService(
                _context,
                _reportRepositoryMock.Object,
                _permissionServiceMock.Object,
                Mock.Of<ILogger<ReportService>>());
        }

        [Fact]
        public async Task GetAvailableReportsAsync_WithValidUser_ShouldReturnTemplates()
        {
            // Arrange
            var userId = "test-user-id";
            var templates = new List<ReportTemplate>
            {
                new ReportTemplate
                {
                    Id = 1,
                    Name = "Printer Report",
                    Category = "printers",
                    EntityType = "Printer",
                    RequiredClaim = "printers.view",
                    IsActive = true
                }
            };

            _reportRepositoryMock
                .Setup(r => r.GetAvailableReportTemplatesAsync(userId))
                .ReturnsAsync(templates);

            // Act
            var result = await _reportService.GetAvailableReportsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var template = result.First();
            template.Name.Should().Be("Printer Report");
            template.Category.Should().Be("printers");
        }

        [Fact]
        public async Task GenerateReportAsync_WithValidRequest_ShouldCreateExecution()
        {
            // Arrange
            var userId = "test-user-id";
            var templateId = 1;
            var request = new ReportRequestDto
            {
                ReportTemplateId = templateId,
                Format = "json",
                Parameters = new Dictionary<string, object> { { "test", "value" } }
            };

            var template = new ReportTemplate
            {
                Id = templateId,
                Name = "Test Report",
                EntityType = "Printer",
                RequiredClaim = "printers.view",
                IsActive = true
            };

            var execution = new ReportExecution
            {
                Id = 1,
                ReportTemplateId = templateId,
                ExecutedByUserId = userId,
                Format = "json",
                Status = "pending"
            };

            _reportRepositoryMock
                .Setup(r => r.GetReportTemplateByIdAsync(templateId))
                .ReturnsAsync(template);

            _permissionServiceMock
                .Setup(p => p.UserHasClaimAsync(userId, "printers.view"))
                .ReturnsAsync(true);

            _reportRepositoryMock
                .Setup(r => r.CreateReportExecutionAsync(It.IsAny<ReportExecution>()))
                .ReturnsAsync(execution);

            // Act
            var result = await _reportService.GenerateReportAsync(request, userId, "127.0.0.1");

            // Assert
            result.Should().NotBeNull();
            result.ExecutionId.Should().Be(1);
            result.ReportTemplateId.Should().Be(templateId);
            result.ReportName.Should().Be("Test Report");
            result.Format.Should().Be("json");
            result.Status.Should().Be("pending");
        }

        [Fact]
        public async Task GenerateReportAsync_WithoutRequiredClaim_ShouldThrowUnauthorized()
        {
            // Arrange
            var userId = "test-user-id";
            var templateId = 1;
            var request = new ReportRequestDto
            {
                ReportTemplateId = templateId,
                Format = "json"
            };

            var template = new ReportTemplate
            {
                Id = templateId,
                Name = "Test Report",
                RequiredClaim = "printers.view",
                IsActive = true
            };

            _reportRepositoryMock
                .Setup(r => r.GetReportTemplateByIdAsync(templateId))
                .ReturnsAsync(template);

            _permissionServiceMock
                .Setup(p => p.UserHasClaimAsync(userId, "printers.view"))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _reportService.GenerateReportAsync(request, userId, "127.0.0.1"));
        }

        [Fact]
        public async Task GetReportHistoryAsync_WithValidUser_ShouldReturnHistory()
        {
            // Arrange
            var userId = "test-user-id";
            var executions = new List<ReportExecution>
            {
                new ReportExecution
                {
                    Id = 1,
                    ReportTemplateId = 1,
                    ExecutedByUserId = userId,
                    Format = "json",
                    Status = "completed",
                    StartedAtUtc = DateTime.UtcNow
                }
            };

            _reportRepositoryMock
                .Setup(r => r.GetUserReportExecutionsAsync(userId, 1, 20))
                .ReturnsAsync(executions);

            // Act
            var result = await _reportService.GetReportHistoryAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var history = result.First();
            history.ExecutionId.Should().Be(1);
            history.Format.Should().Be("json");
            history.Status.Should().Be("completed");
        }

        [Fact]
        public async Task GetReportExecutionAsync_WithValidExecution_ShouldReturnDetails()
        {
            // Arrange
            var userId = "test-user-id";
            var executionId = 1;

            var execution = new ReportExecution
            {
                Id = executionId,
                ReportTemplateId = 1,
                ExecutedByUserId = userId,
                Format = "csv",
                Status = "completed",
                RecordCount = 10,
                StartedAtUtc = DateTime.UtcNow
            };

            _reportRepositoryMock
                .Setup(r => r.GetReportExecutionByIdAsync(executionId))
                .ReturnsAsync(execution);

            // Act
            var result = await _reportService.GetReportExecutionAsync(executionId, userId);

            // Assert
            result.Should().NotBeNull();
            result!.ExecutionId.Should().Be(executionId);
            result.Format.Should().Be("csv");
            result.Status.Should().Be("completed");
            result.RecordCount.Should().Be(10);
        }

        [Fact]
        public async Task GetReportExecutionAsync_WithNonExistentExecution_ShouldReturnNull()
        {
            // Arrange
            var userId = "test-user-id";
            var executionId = 999;

            _reportRepositoryMock
                .Setup(r => r.GetReportExecutionByIdAsync(executionId))
                .ReturnsAsync((ReportExecution?)null);

            // Act
            var result = await _reportService.GetReportExecutionAsync(executionId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CancelReportExecutionAsync_WithValidExecution_ShouldReturnTrue()
        {
            // Arrange
            var userId = "test-user-id";
            var executionId = 1;

            var execution = new ReportExecution
            {
                Id = executionId,
                ExecutedByUserId = userId,
                Status = "running"
            };

            _reportRepositoryMock
                .Setup(r => r.GetReportExecutionByIdAsync(executionId))
                .ReturnsAsync(execution);

            _reportRepositoryMock
                .Setup(r => r.UpdateReportExecutionAsync(execution))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _reportService.CancelReportExecutionAsync(executionId, userId);

            // Assert
            result.Should().BeTrue();
            execution.Status.Should().Be("cancelled");
        }

        [Fact]
        public async Task CancelReportExecutionAsync_WithCompletedExecution_ShouldReturnFalse()
        {
            // Arrange
            var userId = "test-user-id";
            var executionId = 1;

            var execution = new ReportExecution
            {
                Id = executionId,
                ExecutedByUserId = userId,
                Status = "completed"
            };

            _reportRepositoryMock
                .Setup(r => r.GetReportExecutionByIdAsync(executionId))
                .ReturnsAsync(execution);

            // Act
            var result = await _reportService.CancelReportExecutionAsync(executionId, userId);

            // Assert
            result.Should().BeFalse();
        }
    }
}
