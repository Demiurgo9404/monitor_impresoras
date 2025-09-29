using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.API.Controllers;

namespace MonitorImpresoras.Tests.Integration
{
    public class AdvancedPredictiveIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AdvancedPredictiveIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SubmitFeedback_WithUserAuth_ShouldProcessFeedback()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var predictionId = 1L;
            var feedbackDto = new
            {
                IsCorrect = false,
                Comment = "El fallo ocurrió antes de lo previsto",
                ProposedCorrection = "La predicción debería haber sido 2 días antes"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/v1/feedback/{predictionId}/feedback", feedbackDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.Message.Should().Contain("procesado exitosamente");
        }

        [Fact]
        public async Task GetAdvancedStatistics_WithAdminAuth_ShouldReturnStatistics()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/feedback/statistics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var statistics = await response.Content.ReadFromJsonAsync<dynamic>();
            statistics.Should().NotBeNull();
            statistics!.TotalFeedback.Should().NotBeNull();
            statistics!.OverallAccuracy.Should().NotBeNull();
            statistics!.AccuracyByType.Should().NotBeNull();
        }

        [Fact]
        public async Task RetrainModel_WithAdminAuth_ShouldReturnRetrainingResult()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.PostAsync("/api/v1/feedback/retrain", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.RetrainingResult.Should().NotBeNull();
            result!.TriggeredBy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPredictionFeedback_WithUserAuth_ShouldReturnFeedbackHistory()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var predictionId = 1L;

            // Act
            var response = await _client.GetAsync($"/api/v1/feedback/{predictionId}/feedback");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
            result!.PredictionId.Should().Be(predictionId);
            result!.TotalFeedback.Should().NotBeNull();
            result!.FeedbackHistory.Should().NotBeNull();
        }

        [Fact]
        public async Task GetModelQualityMetrics_WithAdminAuth_ShouldReturnQualityMetrics()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/feedback/model-quality");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var metrics = await response.Content.ReadFromJsonAsync<dynamic>();
            metrics.Should().NotBeNull();
            metrics!.ModelVersion.Should().NotBeNull();
            metrics!.CurrentAccuracy.Should().NotBeNull();
            metrics!.AccuracyTrend.Should().NotBeNull();
            metrics!.PredictionTypes.Should().NotBeNull();
        }

        [Fact]
        public async Task SubmitFeedback_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/feedback/1/feedback",
                new { IsCorrect = false, Comment = "Test" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RetrainModel_AsUser_ShouldReturnForbidden()
        {
            // Arrange - Login as regular user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.PostAsync("/api/v1/feedback/retrain", null);

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
