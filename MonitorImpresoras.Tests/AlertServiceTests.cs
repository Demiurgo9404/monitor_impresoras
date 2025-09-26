using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MonitorImpresoras.Tests.Services
{
    public class AlertServiceTests
    {
        private readonly Mock<IRepository<Alert>> _mockAlertRepository;
        private readonly Mock<IPrinterRepository> _mockPrinterRepository;
        private readonly Mock<ILogger<AlertService>> _mockLogger;
        private readonly AlertService _alertService;

        public AlertServiceTests()
        {
            _mockAlertRepository = new Mock<IRepository<Alert>>();
            _mockPrinterRepository = new Mock<IPrinterRepository>();
            _mockLogger = new Mock<ILogger<AlertService>>();
            _alertService = new AlertService(_mockAlertRepository.Object, _mockPrinterRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SendAlertAsync_ShouldCreateAlert_WhenValidMessageProvided()
        {
            // Arrange
            var message = "Test alert message";
            var severity = "High";
            var alert = new Alert();

            _mockAlertRepository.Setup(repo => repo.AddAsync(It.IsAny<Alert>()))
                .Returns(Task.CompletedTask);
            _mockAlertRepository.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _alertService.SendAlertAsync(message, severity);

            // Assert
            _mockAlertRepository.Verify(repo => repo.AddAsync(It.IsAny<Alert>()), Times.Once);
            _mockAlertRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mockLogger.Verify(logger => logger.LogInformation(
                "Alert sent successfully. Type: {AlertType}, Severity: {Severity}, Message: {Message}",
                It.IsAny<AlertType>(), AlertSeverity.High, message), Times.Once);
        }

        [Fact]
        public async Task SendAlertAsync_ShouldThrowArgumentException_WhenMessageIsEmpty()
        {
            // Arrange
            var emptyMessage = "";
            var severity = "Medium";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _alertService.SendAlertAsync(emptyMessage, severity));

            Assert.Contains("Alert message cannot be empty", exception.Message);
        }

        [Fact]
        public async Task SendAlertAsync_ShouldThrowArgumentException_WhenMessageIsWhitespace()
        {
            // Arrange
            var whitespaceMessage = "   ";
            var severity = "Low";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _alertService.SendAlertAsync(whitespaceMessage, severity));

            Assert.Contains("Alert message cannot be empty", exception.Message);
        }

        [Fact]
        public async Task SendAlertAsync_ShouldUseDefaultSeverity_WhenInvalidSeverityProvided()
        {
            // Arrange
            var message = "Valid message";
            var invalidSeverity = "InvalidSeverity";
            var alert = new Alert();

            _mockAlertRepository.Setup(repo => repo.AddAsync(It.IsAny<Alert>()))
                .Returns(Task.CompletedTask);
            _mockAlertRepository.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _alertService.SendAlertAsync(message, invalidSeverity);

            // Assert
            _mockAlertRepository.Verify(repo => repo.AddAsync(It.Is<Alert>(a =>
                a.Severity == AlertSeverity.Medium)), Times.Once); // Should default to Medium
        }

        [Fact]
        public async Task SendAlertAsync_ShouldParseValidSeverity_WhenProvided()
        {
            // Arrange
            var message = "Valid message";
            var validSeverity = "High";
            var alert = new Alert();

            _mockAlertRepository.Setup(repo => repo.AddAsync(It.IsAny<Alert>()))
                .Returns(Task.CompletedTask);
            _mockAlertRepository.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _alertService.SendAlertAsync(message, validSeverity);

            // Assert
            _mockAlertRepository.Verify(repo => repo.AddAsync(It.Is<Alert>(a =>
                a.Severity == AlertSeverity.High)), Times.Once);
        }

        [Fact]
        public async Task SendAlertAsync_ShouldHandleRepositoryException()
        {
            // Arrange
            var message = "Test message";
            var severity = "Medium";

            _mockAlertRepository.Setup(repo => repo.AddAsync(It.IsAny<Alert>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                _alertService.SendAlertAsync(message, severity));

            Assert.Contains("Failed to send alert", exception.Message);
            _mockLogger.Verify(logger => logger.LogError(
                It.IsAny<Exception>(),
                "Error sending alert with message: {Message}",
                message), Times.Once);
        }
    }
}
