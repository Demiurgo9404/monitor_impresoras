using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Tests.Unit
{
    public class ScheduledReportServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IReportService> _reportServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly ScheduledReportService _scheduledReportService;

        public ScheduledReportServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Setup mocks
            _reportServiceMock = new Mock<IReportService>();
            _emailServiceMock = new Mock<IEmailService>();

            // Setup ScheduledReportService
            _scheduledReportService = new ScheduledReportService(
                _context,
                _reportServiceMock.Object,
                _emailServiceMock.Object,
                Mock.Of<ILogger<ScheduledReportService>>());
        }

        [Fact]
        public async Task GetUserScheduledReportsAsync_WithValidUser_ShouldReturnScheduledReports()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new User { Id = userId, UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var template = new ReportTemplate
            {
                Id = 1,
                Name = "Test Report",
                Category = "printers",
                EntityType = "Printer",
                IsActive = true
            };
            await _context.ReportTemplates.AddAsync(template);

            var scheduledReport = new ScheduledReport
            {
                Id = 1,
                ReportTemplateId = 1,
                CreatedByUserId = userId,
                Name = "Daily Report",
                CronExpression = "0 9 * * *",
                Format = "pdf",
                IsActive = true
            };
            await _context.ScheduledReports.AddAsync(scheduledReport);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduledReportService.GetUserScheduledReportsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var report = result.First();
            report.Name.Should().Be("Daily Report");
            report.Format.Should().Be("pdf");
        }

        [Fact]
        public async Task CreateScheduledReportAsync_WithValidData_ShouldReturnScheduledReport()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new User { Id = userId, UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var template = new ReportTemplate
            {
                Id = 1,
                Name = "Test Report",
                Category = "printers",
                EntityType = "Printer",
                IsActive = true
            };
            await _context.ReportTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var request = new CreateScheduledReportDto
            {
                ReportTemplateId = 1,
                Name = "Weekly Report",
                Description = "Weekly printer report",
                CronExpression = "0 9 * * MON",
                Format = "excel",
                Recipients = "admin@company.com,manager@company.com"
            };

            // Act
            var result = await _scheduledReportService.CreateScheduledReportAsync(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Weekly Report");
            result.Format.Should().Be("excel");
            result.ReportTemplateId.Should().Be(1);

            // Verify it's saved in database
            var savedReport = await _context.ScheduledReports.FirstOrDefaultAsync();
            savedReport.Should().NotBeNull();
            savedReport!.Name.Should().Be("Weekly Report");
        }

        [Fact]
        public async Task CreateScheduledReportAsync_WithInvalidCronExpression_ShouldThrowException()
        {
            // Arrange
            var userId = "test-user-id";
            var request = new CreateScheduledReportDto
            {
                ReportTemplateId = 1,
                Name = "Invalid Cron Report",
                CronExpression = "invalid-cron-expression"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _scheduledReportService.CreateScheduledReportAsync(request, userId));
        }

        [Fact]
        public async Task UpdateScheduledReportAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new User { Id = userId, UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var template = new ReportTemplate
            {
                Id = 1,
                Name = "Test Report",
                Category = "printers",
                EntityType = "Printer",
                IsActive = true
            };
            await _context.ReportTemplates.AddAsync(template);

            var scheduledReport = new ScheduledReport
            {
                Id = 1,
                ReportTemplateId = 1,
                CreatedByUserId = userId,
                Name = "Original Name",
                CronExpression = "0 9 * * *",
                Format = "pdf",
                IsActive = true
            };
            await _context.ScheduledReports.AddAsync(scheduledReport);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateScheduledReportDto
            {
                Name = "Updated Name",
                CronExpression = "0 10 * * *",
                Format = "excel",
                IsActive = false
            };

            // Act
            var result = await _scheduledReportService.UpdateScheduledReportAsync(1, updateRequest, userId);

            // Assert
            result.Should().BeTrue();

            var updatedReport = await _context.ScheduledReports.FirstOrDefaultAsync();
            updatedReport.Should().NotBeNull();
            updatedReport!.Name.Should().Be("Updated Name");
            updatedReport.CronExpression.Should().Be("0 10 * * *");
            updatedReport.Format.Should().Be("excel");
            updatedReport.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteScheduledReportAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new User { Id = userId, UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var scheduledReport = new ScheduledReport
            {
                Id = 1,
                ReportTemplateId = 1,
                CreatedByUserId = userId,
                Name = "Report to Delete",
                IsActive = true
            };
            await _context.ScheduledReports.AddAsync(scheduledReport);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduledReportService.DeleteScheduledReportAsync(1, userId);

            // Assert
            result.Should().BeTrue();

            var deletedReport = await _context.ScheduledReports.FirstOrDefaultAsync();
            deletedReport.Should().NotBeNull();
            deletedReport!.IsActive.Should().BeFalse();
        }

        [Fact]
        public void CalculateNextExecution_WithValidCron_ShouldReturnNextExecution()
        {
            // Arrange
            var cronExpression = "0 9 * * MON"; // Every Monday at 9 AM

            // Act
            var result = _scheduledReportService.CalculateNextExecution(cronExpression);

            // Assert
            result.Should().BeAfter(DateTime.UtcNow);
            result.DayOfWeek.Should().Be(DayOfWeek.Monday);
            result.Hour.Should().Be(9);
            result.Minute.Should().Be(0);
        }

        [Fact]
        public void CalculateNextExecution_WithInvalidCron_ShouldReturnDefault()
        {
            // Arrange
            var invalidCronExpression = "invalid-cron";

            // Act
            var result = _scheduledReportService.CalculateNextExecution(invalidCronExpression);

            // Assert
            result.Should().BeAfter(DateTime.UtcNow);
        }
    }
}
