using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.API.Controllers;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Tests.Integration
{
    public class NotificationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public NotificationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SendCriticalNotification_WithValidAuth_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var notification = new
            {
                Title = "Test Critical Alert",
                Message = "This is a test critical notification",
                Recipients = new[] { "admin@company.com" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/notifications/critical", notification);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            ((bool)result!.Success).Should().BeTrue();
        }

        [Fact]
        public async Task SendWarningNotification_WithManagerAuth_ShouldReturnSuccess()
        {
            // Arrange - Login as manager
            var loginResponse = await LoginAsManager();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var notification = new
            {
                Title = "Test Warning Alert",
                Message = "This is a test warning notification",
                Recipients = new[] { "manager@company.com" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/notifications/warning", notification);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            ((bool)result!.Success).Should().BeTrue();
        }

        [Fact]
        public async Task SendInfoNotification_WithUserAuth_ShouldReturnSuccess()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var notification = new
            {
                Title = "Test Info Alert",
                Message = "This is a test info notification",
                Recipients = new[] { "user@company.com" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/notifications/info", notification);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            ((bool)result!.Success).Should().BeTrue();
        }

        [Fact]
        public async Task SendCriticalNotification_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/notifications/critical",
                new { Title = "Test", Message = "Test" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SendWarningNotification_AsUser_ShouldReturnForbidden()
        {
            // Arrange - Login as regular user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/notifications/warning",
                new { Title = "Test", Message = "Test" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetNotificationStatistics_WithAdminAuth_ShouldReturnStatistics()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/notifications/statistics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var statistics = await response.Content.ReadFromJsonAsync<dynamic>();
            statistics.Should().NotBeNull();
            statistics!.TotalSent.Should().NotBeNull();
            statistics!.Successful.Should().NotBeNull();
            statistics!.Failed.Should().NotBeNull();
        }

        [Fact]
        public async Task TestNotificationConfiguration_WithAdminAuth_ShouldReturnTestResults()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var testConfig = new
            {
                Channels = new[] { "Email" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/notifications/test", testConfig);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.TotalTests.Should().NotBeNull();
        }

        [Fact]
        public async Task GetNotificationHistory_WithAdminAuth_ShouldReturnHistory()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/notifications/history");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var history = await response.Content.ReadFromJsonAsync<IEnumerable<dynamic>>();
            history.Should().NotBeNull();
        }

        private async Task<LoginResponseDto> LoginAsAdmin()
        {
            var loginDto = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>()!;
        }

        private async Task<LoginResponseDto> LoginAsManager()
        {
            var loginDto = new LoginRequestDto
            {
                Username = "manager@monitorimpresoras.com",
                Password = "Manager123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>()!;
        }

        private async Task<LoginResponseDto> LoginAsUser()
        {
            var loginDto = new LoginRequestDto
            {
                Username = "user@monitorimpresoras.com",
                Password = "User123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>()!;
        }
    }
}
