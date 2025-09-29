using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.API.Controllers;

namespace MonitorImpresoras.Tests.Integration
{
    public class DeploymentIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DeploymentIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthEndpoint_ShouldReturnHealthy()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var health = await response.Content.ReadFromJsonAsync<dynamic>();
            health.Should().NotBeNull();
            ((string)health!.status).Should().Be("Healthy");
        }

        [Fact]
        public async Task HealthExtendedEndpoint_ShouldReturnExtendedHealth()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health/extended");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var health = await response.Content.ReadFromJsonAsync<dynamic>();
            health.Should().NotBeNull();
            health!.status.Should().NotBeNull();
            health!.database.Should().NotBeNull();
            health!.scheduledReports.Should().NotBeNull();
            health!.system.Should().NotBeNull();
        }

        [Fact]
        public async Task HealthSecureEndpoint_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health/secure");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task HealthReadyEndpoint_ShouldReturnReady()
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
        public async Task HealthLiveEndpoint_ShouldReturnAlive()
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
        public async Task MetricsEndpoint_ShouldReturnPrometheusMetrics()
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
        public async Task ReportsAvailableEndpoint_ShouldReturnAvailableReports()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/reports/available");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var reports = await response.Content.ReadFromJsonAsync<IEnumerable<dynamic>>();
            reports.Should().NotBeNull();
            reports.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SwaggerEndpoint_ShouldReturnSwaggerUI()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Swagger");
            content.Should().Contain("Monitor Impresoras API");
        }

        [Fact]
        public async Task ApiVersion_ShouldBeIncludedInResponseHeaders()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Should().ContainKey("api-supported-versions");
        }

        [Fact]
        public async Task Application_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_client.GetAsync("/api/v1/health"));
            }

            // Act
            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        }

        [Fact]
        public async Task Application_ShouldHaveProperCorsHeaders()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        }
    }
}
