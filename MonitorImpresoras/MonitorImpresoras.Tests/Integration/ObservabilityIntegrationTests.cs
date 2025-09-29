using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Tests.Integration
{
    public class ObservabilityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ObservabilityIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetBasicHealth_WithoutAuth_ShouldReturnHealthy()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var health = await response.Content.ReadFromJsonAsync<HealthCheckDto>();
            health.Should().NotBeNull();
            health!.Status.Should().Be("Healthy");
            health.Checks.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetExtendedHealth_WithoutAuth_ShouldReturnHealthy()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health/extended");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var health = await response.Content.ReadFromJsonAsync<ExtendedHealthCheckDto>();
            health.Should().NotBeNull();
            health!.Status.Should().Be("Healthy");
            health.Database.Should().NotBeNull();
            health.ScheduledReports.Should().NotBeNull();
            health.System.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSecureHealth_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health/secure");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSecureHealth_WithValidAuth_ShouldReturnExtendedHealth()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/health/secure");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var health = await response.Content.ReadFromJsonAsync<ExtendedHealthCheckDto>();
            health.Should().NotBeNull();
            health!.Status.Should().Be("Healthy");
            health.Environment.Should().NotBeNull();
        }

        [Fact]
        public async Task GetReadiness_WithoutAuth_ShouldReturnReady()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health/ready");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var readiness = await response.Content.ReadFromJsonAsync<dynamic>();
            readiness.Should().NotBeNull();
            ((string)readiness!.Status).Should().Be("Healthy");
        }

        [Fact]
        public async Task GetLiveness_WithoutAuth_ShouldReturnAlive()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health/live");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var liveness = await response.Content.ReadFromJsonAsync<dynamic>();
            liveness.Should().NotBeNull();
            ((string)liveness!.Status).Should().Be("Alive");
        }

        [Fact]
        public async Task GetMetrics_WithoutAuth_ShouldReturnMetrics()
        {
            // Act
            var response = await _client.GetAsync("/metrics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("# Custom application metrics");
        }

        [Fact]
        public async Task GetSystemEvents_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/audit/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSystemEvents_WithValidAuth_ShouldReturnEvents()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/audit/events");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var events = await response.Content.ReadFromJsonAsync<IEnumerable<SystemEventDto>>();
            events.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSystemEventStatistics_WithValidAuth_ShouldReturnStatistics()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/audit/statistics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var statistics = await response.Content.ReadFromJsonAsync<SystemEventStatisticsDto>();
            statistics.Should().NotBeNull();
        }

        [Fact]
        public async Task CleanupOldEvents_WithValidAuth_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.DeleteAsync("/api/v1/audit/cleanup?retentionDays=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
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
    }
}
