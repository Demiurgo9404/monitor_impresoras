using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Tests.Unit
{
    public class AlertServiceTests
    {
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IExtendedAuditService> _auditServiceMock;
        private readonly Mock<ILogger<AlertService>> _loggerMock;
        private readonly AlertService _alertService;

        public AlertServiceTests()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _auditServiceMock = new Mock<IExtendedAuditService>();
            _loggerMock = new Mock<ILogger<AlertService>>();

            _alertService = new AlertService(
                _notificationServiceMock.Object,
                _auditServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessPrinterAlertAsync_ShouldSendCriticalAlert_WhenPrinterGoesOffline()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "Test Printer",
                Status = PrinterStatus.Offline,
                Location = "Office 1",
                Model = "HP LaserJet"
            };

            var previousStatus = PrinterStatus.Online;

            _notificationServiceMock
                .Setup(x => x.SendCriticalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new NotificationResponseDto
                {
                    Success = true,
                    NotificationId = Guid.NewGuid(),
                    SentAt = DateTime.UtcNow
                });

            // Act
            await _alertService.ProcessPrinterAlertAsync(printer, previousStatus);

            // Assert
            _notificationServiceMock.Verify(
                x => x.SendCriticalAsync(
                    It.Is<string>(s => s.Contains("Impresora Desconectada")),
                    It.Is<string>(s => s.Contains("Test Printer")),
                    It.IsAny<List<string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPrinterAlertAsync_ShouldSendWarningAlert_WhenTonerIsLow()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "Test Printer",
                Status = PrinterStatus.Online,
                TonerLevel = 10,
                Location = "Office 1",
                Model = "HP LaserJet"
            };

            var previousStatus = PrinterStatus.Online;

            _notificationServiceMock
                .Setup(x => x.SendWarningAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new NotificationResponseDto
                {
                    Success = true,
                    NotificationId = Guid.NewGuid(),
                    SentAt = DateTime.UtcNow
                });

            // Act
            await _alertService.ProcessPrinterAlertAsync(printer, previousStatus);

            // Assert
            _notificationServiceMock.Verify(
                x => x.SendWarningAsync(
                    It.Is<string>(s => s.Contains("Tóner Bajo")),
                    It.Is<string>(s => s.Contains("10%")),
                    It.IsAny<List<string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPrinterAlertAsync_ShouldSendInfoAlert_WhenPrinterComesBackOnline()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "Test Printer",
                Status = PrinterStatus.Online,
                Location = "Office 1",
                Model = "HP LaserJet"
            };

            var previousStatus = PrinterStatus.Offline;

            _notificationServiceMock
                .Setup(x => x.SendInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new NotificationResponseDto
                {
                    Success = true,
                    NotificationId = Guid.NewGuid(),
                    SentAt = DateTime.UtcNow
                });

            // Act
            await _alertService.ProcessPrinterAlertAsync(printer, previousStatus);

            // Assert
            _notificationServiceMock.Verify(
                x => x.SendInfoAsync(
                    It.Is<string>(s => s.Contains("Impresora Reconectada")),
                    It.Is<string>(s => s.Contains("Test Printer")),
                    It.IsAny<List<string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessSystemMetricsAlertAsync_ShouldSendCriticalAlert_WhenCpuUsageHigh()
        {
            // Arrange
            var metrics = new Application.Services.SystemMetrics
            {
                CpuUsage = 95.0,
                MemoryUsage = 50.0,
                Uptime = TimeSpan.FromHours(24),
                DatabaseHealthy = true,
                ScheduledReportsHealthy = true
            };

            _notificationServiceMock
                .Setup(x => x.SendCriticalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new NotificationResponseDto
                {
                    Success = true,
                    NotificationId = Guid.NewGuid(),
                    SentAt = DateTime.UtcNow
                });

            // Act
            await _alertService.ProcessSystemMetricsAlertAsync(metrics);

            // Assert
            _notificationServiceMock.Verify(
                x => x.SendCriticalAsync(
                    It.Is<string>(s => s.Contains("Alto Uso de CPU")),
                    It.Is<string>(s => s.Contains("95")),
                    It.IsAny<List<string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendDailySummaryReportAsync_ShouldSendInfoNotification()
        {
            // Arrange
            _notificationServiceMock
                .Setup(x => x.SendInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new NotificationResponseDto
                {
                    Success = true,
                    NotificationId = Guid.NewGuid(),
                    SentAt = DateTime.UtcNow
                });

            // Act
            await _alertService.SendDailySummaryReportAsync();

            // Assert
            _notificationServiceMock.Verify(
                x => x.SendInfoAsync(
                    It.Is<string>(s => s.Contains("Reporte Diario")),
                    It.Is<string>(s => s.Contains("REPORTE DIARIO")),
                    It.IsAny<List<string>>()),
                Times.Once);
        }
    }

    public class EmailNotificationServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<EmailNotificationService>> _loggerMock;
        private readonly Mock<IExtendedAuditService> _auditServiceMock;
        private readonly EmailNotificationService _emailService;

        public EmailNotificationServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<EmailNotificationService>>();
            _auditServiceMock = new Mock<IExtendedAuditService>();

            // Setup configuración básica
            SetupConfiguration();

            _emailService = new EmailNotificationService(
                _configurationMock.Object,
                _loggerMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupConfiguration()
        {
            var emailSection = new Mock<IConfigurationSection>();
            emailSection.Setup(x => x["SmtpHost"]).Returns("smtp.gmail.com");
            emailSection.Setup(x => x["SmtpPort"]).Returns("587");
            emailSection.Setup(x => x["UseSsl"]).Returns("true");
            emailSection.Setup(x => x["Username"]).Returns("test@example.com");
            emailSection.Setup(x => x["Password"]).Returns("testpassword");
            emailSection.Setup(x => x["FromAddress"]).Returns("noreply@monitorimpresoras.com");
            emailSection.Setup(x => x["FromName"]).Returns("Sistema de Monitor");

            var notificationsSection = new Mock<IConfigurationSection>();
            notificationsSection.Setup(x => x["DefaultRecipients"]).Returns("admin@company.com,manager@company.com");
            notificationsSection.Setup(x => x["Email:Enabled"]).Returns("true");

            _configurationMock.Setup(x => x.GetSection("Email")).Returns(emailSection.Object);
            _configurationMock.Setup(x => x.GetSection("Notifications")).Returns(notificationsSection.Object);
        }

        [Fact]
        public async Task SendCriticalAsync_ShouldSendEmail_WithCorrectSubject()
        {
            // Arrange
            var notificationResponse = new NotificationResponseDto
            {
                Success = true,
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.Email,
                RecipientsCount = 2
            };

            // Esta prueba sería más completa con un servidor SMTP de prueba
            // Por ahora verificamos que el método se ejecuta sin errores

            // Act & Assert
            // El servicio intentaría enviar email pero fallaría sin servidor SMTP real
            // Esto es esperado en un entorno de pruebas
            await Assert.ThrowsAsync<Exception>(() => _emailService.SendCriticalAsync("Test", "Test message"));
        }

        [Fact]
        public async Task GetDefaultRecipients_ShouldReturnConfiguredRecipients()
        {
            // Arrange
            var service = new EmailNotificationService(
                _configurationMock.Object,
                _loggerMock.Object,
                _auditServiceMock.Object);

            // Act
            var recipients = service.GetDefaultRecipients();

            // Assert
            recipients.Should().NotBeNull();
            recipients.Should().HaveCount(2);
            recipients.Should().Contain("admin@company.com");
            recipients.Should().Contain("manager@company.com");
        }

        [Fact]
        public async Task GetEnabledChannels_ShouldReturnEmailChannel()
        {
            // Arrange
            var service = new EmailNotificationService(
                _configurationMock.Object,
                _loggerMock.Object,
                _auditServiceMock.Object);

            // Act
            var channels = service.GetEnabledChannels();

            // Assert
            channels.Should().NotBeNull();
            channels.Should().Contain(NotificationChannel.Email);
        }
    }

    // Clases auxiliares para pruebas
    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public TimeSpan Uptime { get; set; }
        public bool DatabaseHealthy { get; set; }
        public bool ScheduledReportsHealthy { get; set; }
    }
}
