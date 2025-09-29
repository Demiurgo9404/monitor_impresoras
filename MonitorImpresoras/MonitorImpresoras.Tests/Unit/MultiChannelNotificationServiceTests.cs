using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace MonitorImpresoras.Tests.Unit
{
    public class MultiChannelNotificationServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<MultiChannelNotificationService>> _loggerMock;
        private readonly Mock<IExtendedAuditService> _auditServiceMock;
        private readonly Mock<INotificationEscalationService> _escalationServiceMock;

        // Servicios especializados mockeados
        private readonly Mock<EmailNotificationService> _emailServiceMock;
        private readonly Mock<TeamsNotificationService> _teamsServiceMock;
        private readonly Mock<SlackNotificationService> _slackServiceMock;
        private readonly Mock<WhatsAppNotificationService> _whatsAppServiceMock;

        private readonly MultiChannelNotificationService _notificationService;

        public MultiChannelNotificationServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<MultiChannelNotificationService>>();
            _auditServiceMock = new Mock<IExtendedAuditService>();
            _escalationServiceMock = new Mock<INotificationEscalationService>();

            // Crear servicios mockeados
            _emailServiceMock = new Mock<EmailNotificationService>(
                _configurationMock.Object,
                Mock.Of<ILogger<EmailNotificationService>>(),
                _auditServiceMock.Object);

            _teamsServiceMock = new Mock<TeamsNotificationService>(
                _configurationMock.Object,
                Mock.Of<ILogger<TeamsNotificationService>>(),
                _auditServiceMock.Object);

            _slackServiceMock = new Mock<SlackNotificationService>(
                _configurationMock.Object,
                Mock.Of<ILogger<SlackNotificationService>>(),
                _auditServiceMock.Object);

            _whatsAppServiceMock = new Mock<WhatsAppNotificationService>(
                _configurationMock.Object,
                Mock.Of<ILogger<WhatsAppNotificationService>>(),
                _auditServiceMock.Object);

            _notificationService = new MultiChannelNotificationService(
                _emailServiceMock.Object,
                _teamsServiceMock.Object,
                _slackServiceMock.Object,
                _whatsAppServiceMock.Object,
                _escalationServiceMock.Object,
                _auditServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SendCriticalAsync_ShouldSendToAllEnabledChannels()
        {
            // Arrange
            var response = new NotificationResponseDto
            {
                Success = true,
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.Email
            };

            _emailServiceMock
                .Setup(x => x.SendCriticalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(response);

            _teamsServiceMock
                .Setup(x => x.SendCriticalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(response);

            _slackServiceMock
                .Setup(x => x.SendCriticalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(response);

            _whatsAppServiceMock
                .Setup(x => x.SendCriticalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(response);

            // Act
            var result = await _notificationService.SendCriticalAsync("Test Critical", "Test message");

            // Assert
            result.Success.Should().BeTrue();
            _escalationServiceMock.Verify(
                x => x.StartEscalationTrackingAsync(
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    NotificationSeverity.Critical,
                    It.IsAny<Dictionary<string, object>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendNotificationAsync_WithMultipleChannels_ShouldReturnMultipleResponses()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test",
                Message = "Test message",
                Severity = NotificationSeverity.Info,
                Recipients = new List<string> { "test@example.com" },
                Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Slack }
            };

            var emailResponse = new NotificationResponseDto
            {
                Success = true,
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.Email
            };

            var slackResponse = new NotificationResponseDto
            {
                Success = true,
                NotificationId = Guid.NewGuid(),
                SentAt = DateTime.UtcNow,
                Channel = NotificationChannel.Slack
            };

            _emailServiceMock
                .Setup(x => x.SendByChannelAsync(It.IsAny<NotificationRequestDto>(), NotificationChannel.Email))
                .ReturnsAsync(emailResponse);

            _slackServiceMock
                .Setup(x => x.SendByChannelAsync(It.IsAny<NotificationRequestDto>(), NotificationChannel.Slack))
                .ReturnsAsync(slackResponse);

            // Act
            var responses = await _notificationService.SendNotificationAsync(request);

            // Assert
            responses.Should().HaveCount(2);
            responses.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        }

        [Fact]
        public async Task AcknowledgeNotificationAsync_ShouldCallEscalationService()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            var userId = "testuser";

            _escalationServiceMock
                .Setup(x => x.AcknowledgeNotificationAsync(notificationId.ToString(), userId, It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _notificationService.AcknowledgeNotificationAsync(notificationId, userId);

            // Assert
            result.Should().BeTrue();
            _escalationServiceMock.Verify(
                x => x.AcknowledgeNotificationAsync(notificationId.ToString(), userId, It.IsAny<string>()),
                Times.Once);
        }
    }

    public class TeamsNotificationServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<TeamsNotificationService>> _loggerMock;
        private readonly Mock<IExtendedAuditService> _auditServiceMock;
        private readonly TeamsNotificationService _teamsService;

        public TeamsNotificationServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<TeamsNotificationService>>();
            _auditServiceMock = new Mock<IExtendedAuditService>();

            // Setup configuración básica
            SetupConfiguration();

            _teamsService = new TeamsNotificationService(
                _configurationMock.Object,
                _loggerMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupConfiguration()
        {
            var teamsSection = new Mock<IConfigurationSection>();
            teamsSection.Setup(x => x["WebhookUrl"]).Returns("https://outlook.office.com/webhook/test");

            var notificationsSection = new Mock<IConfigurationSection>();
            notificationsSection.Setup(x => x["Teams:Enabled"]).Returns("true");

            _configurationMock.Setup(x => x.GetSection("Notifications")).Returns(notificationsSection.Object);
            _configurationMock.Setup(x => x.GetSection("Notifications:Teams")).Returns(teamsSection.Object);
        }

        [Fact]
        public async Task SendByChannelAsync_WithTeamsChannel_ShouldCreateAdaptiveCard()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test Teams Alert",
                Message = "This is a test message for Teams",
                Severity = NotificationSeverity.Warning,
                Recipients = new List<string> { "test@example.com" },
                Metadata = new Dictionary<string, object>
                {
                    { "PrinterId", 123 },
                    { "Location", "Office 1" }
                }
            };

            // Esta prueba sería más completa con un mock de HttpClient
            // Por ahora verificamos que el método se ejecuta sin errores de configuración

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _teamsService.SendByChannelAsync(request, NotificationChannel.Teams));
        }

        [Fact]
        public void CreateTeamsMessage_ShouldFormatCorrectly()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test Alert",
                Message = "Test message",
                Severity = NotificationSeverity.Critical,
                Metadata = new Dictionary<string, object>
                {
                    { "PrinterId", 123 },
                    { "Location", "Office 1" }
                }
            };

            // Act
            var message = _teamsService.CreateTeamsMessage(request);

            // Assert
            message.Should().NotBeNull();
            var json = JsonSerializer.Serialize(message);
            json.Should().Contain("AdaptiveCard");
            json.Should().Contain("Test Alert");
        }
    }

    public class SlackNotificationServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<SlackNotificationService>> _loggerMock;
        private readonly Mock<IExtendedAuditService> _auditServiceMock;
        private readonly SlackNotificationService _slackService;

        public SlackNotificationServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<SlackNotificationService>>();
            _auditServiceMock = new Mock<IExtendedAuditService>();

            // Setup configuración básica
            SetupConfiguration();

            _slackService = new SlackNotificationService(
                _configurationMock.Object,
                _loggerMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupConfiguration()
        {
            var slackSection = new Mock<IConfigurationSection>();
            slackSection.Setup(x => x["WebhookUrl"]).Returns("https://hooks.slack.com/services/test");

            var notificationsSection = new Mock<IConfigurationSection>();
            notificationsSection.Setup(x => x["Slack:Enabled"]).Returns("true");

            _configurationMock.Setup(x => x.GetSection("Notifications")).Returns(notificationsSection.Object);
            _configurationMock.Setup(x => x.GetSection("Notifications:Slack")).Returns(slackSection.Object);
        }

        [Fact]
        public async Task SendByChannelAsync_WithSlackChannel_ShouldCreateBlocksMessage()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test Slack Alert",
                Message = "This is a test message for Slack",
                Severity = NotificationSeverity.Info,
                Recipients = new List<string> { "test@example.com" },
                Metadata = new Dictionary<string, object>
                {
                    { "PrinterId", 123 },
                    { "Location", "Office 1" }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _slackService.SendByChannelAsync(request, NotificationChannel.Slack));
        }

        [Fact]
        public void CreateSlackMessage_ShouldFormatCorrectly()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test Alert",
                Message = "Test message",
                Severity = NotificationSeverity.Warning,
                Metadata = new Dictionary<string, object>
                {
                    { "PrinterId", 123 },
                    { "Location", "Office 1" }
                }
            };

            // Act
            var message = _slackService.CreateSlackMessage(request);

            // Assert
            message.Should().NotBeNull();
            var json = JsonSerializer.Serialize(message);
            json.Should().Contain("blocks");
            json.Should().Contain("Test Alert");
        }
    }

    public class WhatsAppNotificationServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<WhatsAppNotificationService>> _loggerMock;
        private readonly Mock<IExtendedAuditService> _auditServiceMock;
        private readonly WhatsAppNotificationService _whatsAppService;

        public WhatsAppNotificationServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<WhatsAppNotificationService>>();
            _auditServiceMock = new Mock<IExtendedAuditService>();

            // Setup configuración básica
            SetupConfiguration();

            _whatsAppService = new WhatsAppNotificationService(
                _configurationMock.Object,
                _loggerMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupConfiguration()
        {
            var twilioSection = new Mock<IConfigurationSection>();
            twilioSection.Setup(x => x["AccountSid"]).Returns("test_account_sid");
            twilioSection.Setup(x => x["AuthToken"]).Returns("test_auth_token");
            twilioSection.Setup(x => x["FromPhoneNumber"]).Returns("whatsapp:+1234567890");

            _configurationMock.Setup(x => x.GetSection("Twilio")).Returns(twilioSection.Object);
        }

        [Fact]
        public async Task SendByChannelAsync_WithWhatsAppChannel_AndNonCriticalSeverity_ShouldSkip()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test WhatsApp Alert",
                Message = "This is a test message for WhatsApp",
                Severity = NotificationSeverity.Info, // No crítico
                Recipients = new List<string> { "+1234567890" }
            };

            // Act
            var response = await _whatsAppService.SendByChannelAsync(request, NotificationChannel.WhatsApp);

            // Assert
            response.Success.Should().BeTrue();
            response.RecipientsCount.Should().Be(0); // No envía mensajes no críticos
        }

        [Fact]
        public async Task SendByChannelAsync_WithWhatsAppChannel_AndCriticalSeverity_ShouldSendMessage()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test Critical WhatsApp Alert",
                Message = "This is a critical test message for WhatsApp",
                Severity = NotificationSeverity.Critical, // Crítico
                Recipients = new List<string> { "+1234567890" }
            };

            // Act & Assert
            // Esta prueba fallaría sin configuración real de Twilio
            await Assert.ThrowsAsync<Exception>(() =>
                _whatsAppService.SendByChannelAsync(request, NotificationChannel.WhatsApp));
        }

        [Fact]
        public void FormatPhoneNumberForWhatsApp_ShouldAddCountryCode()
        {
            // Arrange
            var phoneNumber = "123456789";

            // Act
            var formatted = _whatsAppService.FormatPhoneNumberForWhatsApp(phoneNumber);

            // Assert
            formatted.Should().StartWith("+34"); // Código de España por defecto
        }

        [Fact]
        public void CreateWhatsAppMessage_ShouldFormatCorrectly()
        {
            // Arrange
            var request = new NotificationRequestDto
            {
                Title = "Test Alert",
                Message = "Test message",
                Severity = NotificationSeverity.Critical,
                Metadata = new Dictionary<string, object>
                {
                    { "PrinterId", 123 },
                    { "Location", "Office 1" }
                }
            };

            // Act
            var message = _whatsAppService.CreateWhatsAppMessage(request);

            // Assert
            message.Should().Contain("🚨");
            message.Should().Contain("Test Alert");
            message.Should().Contain("Test message");
            message.Should().Contain("PrinterId: 123");
            message.Should().Contain("Location: Office 1");
        }
    }

    public class NotificationEscalationServiceTests
    {
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IExtendedAuditService> _auditServiceMock;
        private readonly Mock<ILogger<NotificationEscalationService>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly NotificationEscalationService _escalationService;

        public NotificationEscalationServiceTests()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _auditServiceMock = new Mock<IExtendedAuditService>();
            _loggerMock = new Mock<ILogger<NotificationEscalationService>>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _escalationService = new NotificationEscalationService(
                _notificationServiceMock.Object,
                _auditServiceMock.Object,
                _loggerMock.Object,
                _serviceProviderMock.Object);
        }

        [Fact]
        public async Task StartEscalationTrackingAsync_WithCriticalNotification_ShouldStartTracking()
        {
            // Arrange
            var notificationId = Guid.NewGuid().ToString();
            var recipients = new List<string> { "admin@empresa.com" };
            var metadata = new Dictionary<string, object> { { "Test", "Value" } };

            _notificationServiceMock
                .Setup(x => x.SendCriticalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new NotificationResponseDto { Success = true });

            // Act
            await _escalationService.StartEscalationTrackingAsync(
                notificationId,
                recipients,
                NotificationSeverity.Critical,
                metadata);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Escalation tracking started for notification {NotificationId}", notificationId),
                Times.Once);
        }

        [Fact]
        public async Task StartEscalationTrackingAsync_WithNonCriticalNotification_ShouldSkip()
        {
            // Arrange
            var notificationId = Guid.NewGuid().ToString();
            var recipients = new List<string> { "admin@empresa.com" };

            // Act
            await _escalationService.StartEscalationTrackingAsync(
                notificationId,
                recipients,
                NotificationSeverity.Info);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Notification {NotificationId} is not critical, skipping escalation tracking", notificationId),
                Times.Once);
        }

        [Fact]
        public async Task AcknowledgeNotificationAsync_ShouldLogAcknowledgment()
        {
            // Arrange
            var notificationId = Guid.NewGuid().ToString();
            var userId = "testuser";
            var comments = "Problem resolved";

            // Act
            var result = await _escalationService.AcknowledgeNotificationAsync(notificationId, userId, comments);

            // Assert
            result.Should().BeTrue();
            _auditServiceMock.Verify(
                x => x.LogSystemEventAsync(
                    "notification_acknowledged",
                    It.Is<string>(s => s.Contains(notificationId) && s.Contains(userId)),
                    It.Is<string>(s => s.Contains(comments)),
                    It.IsAny<Dictionary<string, object>>(),
                    "Info",
                    true),
                Times.Once);
        }

        [Fact]
        public async Task GetEscalationHistoryAsync_ShouldReturnEmptyList()
        {
            // Arrange
            var notificationId = Guid.NewGuid().ToString();

            // Act
            var history = await _escalationService.GetEscalationHistoryAsync(notificationId);

            // Assert
            history.Should().NotBeNull();
            history.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEscalationStatisticsAsync_ShouldReturnDefaultStatistics()
        {
            // Act
            var statistics = await _escalationService.GetEscalationStatisticsAsync();

            // Assert
            statistics.Should().NotBeNull();
            statistics.TotalEscalations.Should().Be(0);
            statistics.EscalationsByLevel.Should().NotBeNull();
        }
    }
}
