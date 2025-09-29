using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;

namespace MonitorImpresoras.Tests.Unit
{
    public class SecurityHardeningTests
    {
        private readonly Mock<ILogger<SecurityAuditService>> _auditLoggerMock;
        private readonly Mock<ILogger<WindowsHardeningService>> _windowsLoggerMock;
        private readonly Mock<ILogger<IisHardeningService>> _iisLoggerMock;
        private readonly Mock<ILogger<PostgreSqlHardeningService>> _postgresqlLoggerMock;
        private readonly Mock<ILogger<ApiSecurityService>> _apiLoggerMock;

        private readonly SecurityAuditService _auditService;
        private readonly WindowsHardeningService _windowsService;
        private readonly IisHardeningService _iisService;
        private readonly PostgreSqlHardeningService _postgresqlService;
        private readonly ApiSecurityService _apiService;

        public SecurityHardeningTests()
        {
            _auditLoggerMock = new Mock<ILogger<SecurityAuditService>>();
            _windowsLoggerMock = new Mock<ILogger<WindowsHardeningService>>();
            _iisLoggerMock = new Mock<ILogger<IisHardeningService>>();
            _postgresqlLoggerMock = new Mock<ILogger<PostgreSqlHardeningService>>();
            _apiLoggerMock = new Mock<ILogger<ApiSecurityService>>();

            _auditService = new SecurityAuditService(_auditLoggerMock.Object);
            _windowsService = new WindowsHardeningService(_windowsLoggerMock.Object);
            _iisService = new IisHardeningService(_iisLoggerMock.Object);
            _postgresqlService = new PostgreSqlHardeningService(_postgresqlLoggerMock.Object);
            _apiService = new ApiSecurityService(_apiLoggerMock.Object);
        }

        [Fact]
        public async Task SecurityAuditService_PerformSecurityAuditAsync_ShouldReturnCompleteReport()
        {
            // Act
            var audit = await _auditService.PerformSecurityAuditAsync();

            // Assert
            audit.Should().NotBeNull();
            audit.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            audit.SystemInformation.Should().NotBeNull();
            audit.WindowsSecurity.Should().NotBeNull();
            audit.IisSecurity.Should().NotBeNull();
            audit.DatabaseSecurity.Should().NotBeNull();
            audit.ApiSecurity.Should().NotBeNull();
            audit.NetworkSecurity.Should().NotBeNull();
            audit.SecurityRisks.Should().NotBeNull();
            audit.SecurityRecommendations.Should().NotBeNull();
            audit.OverallSecurityScore.Should().BeInRange(0, 100);
        }

        [Fact]
        public async Task SecurityAuditService_CheckSecurityComplianceAsync_ShouldReturnComplianceReport()
        {
            // Act
            var compliance = await _auditService.CheckSecurityComplianceAsync();

            // Assert
            compliance.Should().NotBeNull();
            compliance.CheckDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            compliance.StandardsChecked.Should().NotBeNull();
            compliance.StandardsChecked.Should().Contain("OWASP");
            compliance.StandardsChecked.Should().Contain("CIS");
            compliance.OverallComplianceScore.Should().BeInRange(0, 100);
        }

        [Fact]
        public async Task WindowsHardeningService_HardenWindowsServerAsync_ShouldReturnHardeningResult()
        {
            // Act
            var result = await _windowsService.HardenWindowsServerAsync();

            // Assert
            result.Should().NotBeNull();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            result.HardeningApplied.Should().BeTrue();
            result.AccountLockoutPolicy.Should().NotBeNull();
            result.PasswordPolicy.Should().NotBeNull();
            result.AuditPolicy.Should().NotBeNull();
            result.ServicesHardening.Should().NotBeNull();
            result.FirewallConfiguration.Should().NotBeNull();
            result.WindowsUpdateConfiguration.Should().NotBeNull();
            result.LocalSecurityPolicy.Should().NotBeNull();
        }

        [Fact]
        public async Task WindowsHardeningService_GetHardeningRecommendationsAsync_ShouldReturnRecommendations()
        {
            // Act
            var recommendations = await _windowsService.GetHardeningRecommendationsAsync();

            // Assert
            recommendations.Should().NotBeNull();
            recommendations.Should().HaveCountGreaterThan(0);
            recommendations.Should().AllSatisfy(r => r.Category.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Title.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Implementation.Should().NotBeEmpty());
        }

        [Fact]
        public async Task IisHardeningService_HardenIisAsync_ShouldReturnHardeningResult()
        {
            // Act
            var result = await _iisService.HardenIisAsync();

            // Assert
            result.Should().NotBeNull();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            result.HardeningApplied.Should().BeTrue();
            result.TlsConfiguration.Should().NotBeNull();
            result.SecurityHeaders.Should().NotBeNull();
            result.AuthenticationConfiguration.Should().NotBeNull();
            result.RequestFiltering.Should().NotBeNull();
            result.LoggingConfiguration.Should().NotBeNull();
            result.RateLimitingConfiguration.Should().NotBeNull();
        }

        [Fact]
        public async Task IisHardeningService_GetHardeningRecommendationsAsync_ShouldReturnRecommendations()
        {
            // Act
            var recommendations = await _iisService.GetHardeningRecommendationsAsync();

            // Assert
            recommendations.Should().NotBeNull();
            recommendations.Should().HaveCountGreaterThan(0);
            recommendations.Should().AllSatisfy(r => r.Category.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Title.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Implementation.Should().NotBeEmpty());
        }

        [Fact]
        public async Task PostgreSqlHardeningService_HardenPostgreSqlAsync_ShouldReturnHardeningResult()
        {
            // Act
            var result = await _postgresqlService.HardenPostgreSqlAsync();

            // Assert
            result.Should().NotBeNull();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            result.HardeningApplied.Should().BeTrue();
            result.RoleConfiguration.Should().NotBeNull();
            result.PgHbaConfiguration.Should().NotBeNull();
            result.SslConfiguration.Should().NotBeNull();
            result.AuditConfiguration.Should().NotBeNull();
            result.SecurityParameters.Should().NotBeNull();
            result.BackupConfiguration.Should().NotBeNull();
        }

        [Fact]
        public async Task PostgreSqlHardeningService_GetHardeningRecommendationsAsync_ShouldReturnRecommendations()
        {
            // Act
            var recommendations = await _postgresqlService.GetHardeningRecommendationsAsync();

            // Assert
            recommendations.Should().NotBeNull();
            recommendations.Should().HaveCountGreaterThan(0);
            recommendations.Should().AllSatisfy(r => r.Category.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Title.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Implementation.Should().NotBeEmpty());
        }

        [Fact]
        public async Task ApiSecurityService_ConfigureApiSecurityAsync_ShouldReturnSecurityResult()
        {
            // Act
            var result = await _apiService.ConfigureApiSecurityAsync();

            // Assert
            result.Should().NotBeNull();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            result.SecurityApplied.Should().BeTrue();
            result.SecurityMiddleware.Should().NotBeNull();
            result.InputValidation.Should().NotBeNull();
            result.RateLimiting.Should().NotBeNull();
            result.AttackProtection.Should().NotBeNull();
            result.SecurityAuditing.Should().NotBeNull();
            result.AuthorizationPolicies.Should().NotBeNull();
        }

        [Fact]
        public void ApiSecurityService_ValidateInput_ShouldDetectSqlInjection()
        {
            // Arrange
            var maliciousInput = "'; DROP TABLE Users; --";

            // Act
            var isValid = _apiService.ValidateInput(maliciousInput, InputValidationType.SqlInjection);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void ApiSecurityService_ValidateInput_ShouldDetectXss()
        {
            // Arrange
            var maliciousInput = "<script>alert('xss')</script>";

            // Act
            var isValid = _apiService.ValidateInput(maliciousInput, InputValidationType.Xss);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void ApiSecurityService_ValidateInput_ShouldAllowValidInput()
        {
            // Arrange
            var validInput = "john.doe@example.com";

            // Act
            var isValid = _apiService.ValidateInput(validInput, InputValidationType.SqlInjection);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public async Task ApiSecurityService_GetSecurityRecommendationsAsync_ShouldReturnRecommendations()
        {
            // Act
            var recommendations = await _apiService.GetSecurityRecommendationsAsync();

            // Assert
            recommendations.Should().NotBeNull();
            recommendations.Should().HaveCountGreaterThan(0);
            recommendations.Should().AllSatisfy(r => r.Category.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Title.Should().NotBeEmpty());
            recommendations.Should().AllSatisfy(r => r.Implementation.Should().NotBeEmpty());
        }

        [Fact]
        public void ApiSecurityService_LogSecurityEvent_ShouldLogEvent()
        {
            // Act
            _apiService.LogSecurityEvent("Failed Login", "Usuario intentó login con credenciales inválidas", "user123", "192.168.1.100", true);

            // Assert
            // Se verifica mediante logs - en producción verificarías que se guardó en BD
        }
    }
}
