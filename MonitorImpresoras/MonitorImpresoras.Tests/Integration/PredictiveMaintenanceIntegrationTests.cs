using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.API.Controllers;

namespace MonitorImpresoras.Tests.Integration
{
    public class PredictiveMaintenanceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PredictiveMaintenanceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPredictionsSummary_WithManagerAuth_ShouldReturnSummary()
        {
            // Arrange - Login as manager
            var loginResponse = await LoginAsManager();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/predictions/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var summary = await response.Content.ReadFromJsonAsync<dynamic>();
            summary.Should().NotBeNull();
            summary!.Timestamp.Should().NotBeNull();
            summary!.TotalPredictions.Should().NotBeNull();
            summary!.PredictionsByType.Should().NotBeNull();
            summary!.PredictionsBySeverity.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPrinterPredictions_WithUserAuth_ShouldReturnPredictions()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var printerId = 1;

            // Act
            var response = await _client.GetAsync($"/api/v1/predictions/{printerId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.PrinterId.Should().Be(printerId);
            result!.TotalPredictions.Should().NotBeNull();
            result!.Predictions.Should().NotBeNull();
        }

        [Fact]
        public async Task GeneratePredictions_WithManagerAuth_ShouldReturnNewPredictions()
        {
            // Arrange - Login as manager
            var loginResponse = await LoginAsManager();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var printerId = 1;

            // Act
            var response = await _client.PostAsync($"/api/v1/predictions/{printerId}/generate", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.PrinterId.Should().Be(printerId);
            result!.PredictionsGenerated.Should().NotBeNull();
            result!.Predictions.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPredictionAccuracy_WithAdminAuth_ShouldReturnAccuracyStats()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/predictions/accuracy");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var accuracy = await response.Content.ReadFromJsonAsync<dynamic>();
            accuracy.Should().NotBeNull();
            accuracy!.TotalPredictions.Should().NotBeNull();
            accuracy!.AccuracyRate.Should().NotBeNull();
            accuracy!.AccuracyByType.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPredictionTrends_WithManagerAuth_ShouldReturnTrends()
        {
            // Arrange - Login as manager
            var loginResponse = await LoginAsManager();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/predictions/trends?days=7");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var trends = await response.Content.ReadFromJsonAsync<dynamic>();
            trends.Should().NotBeNull();
            trends!.PeriodDays.Should().Be(7);
            trends!.PredictionTrends.Should().NotBeNull();
            trends!.AccuracyTrend.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPredictionsSummary_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/predictions/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GeneratePredictions_AsUser_ShouldReturnForbidden()
        {
            // Arrange - Login as regular user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.PostAsync("/api/v1/predictions/1/generate", null);

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
