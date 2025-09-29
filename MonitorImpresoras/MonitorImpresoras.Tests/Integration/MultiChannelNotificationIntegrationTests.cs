using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.API.Controllers;

namespace MonitorImpresoras.Tests.Integration
{
    public class MultiChannelNotificationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public MultiChannelNotificationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAlertsDashboard_WithAdminAuth_ShouldReturnDashboard()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/alerts/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dashboard = await response.Content.ReadFromJsonAsync<dynamic>();
            dashboard.Should().NotBeNull();
            dashboard!.Timestamp.Should().NotBeNull();
            dashboard!.Summary.Should().NotBeNull();
            dashboard!.RecentAlerts.Should().NotBeNull();
            dashboard!.EscalationStatus.Should().NotBeNull();
            dashboard!.ChannelStatus.Should().NotBeNull();
        }

        [Fact]
        public async Task SendTestAlert_WithAdminAuth_ShouldReturnTestResults()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var testAlert = new
            {
                Channels = new[] { "Email" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/alerts/test", testAlert);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.TotalChannels.Should().NotBeNull();
            result!.SuccessfulChannels.Should().NotBeNull();
            result!.FailedChannels.Should().NotBeNull();
            result!.Results.Should().NotBeNull();
        }

        [Fact]
        public async Task GetEscalationStatistics_WithAdminAuth_ShouldReturnStatistics()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/alerts/escalation-statistics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var statistics = await response.Content.ReadFromJsonAsync<dynamic>();
            statistics.Should().NotBeNull();
            statistics!.TotalEscalations.Should().NotBeNull();
            statistics!.EscalationsByLevel.Should().NotBeNull();
        }

        [Fact]
        public async Task AcknowledgeAlert_WithUserAuth_ShouldReturnSuccess()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var notificationId = Guid.NewGuid();
            var acknowledgment = new
            {
                Comments = "Problem resolved by checking printer connection"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/alerts/{notificationId}/acknowledge", acknowledgment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.Message.Should().Contain("reconocida exitosamente");
        }

        [Fact]
        public async Task GetAlertsDashboard_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/alerts/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SendTestAlert_AsUser_ShouldReturnForbidden()
        {
            // Arrange - Login as regular user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/alerts/test",
                new { Channels = new[] { "Email" } });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
