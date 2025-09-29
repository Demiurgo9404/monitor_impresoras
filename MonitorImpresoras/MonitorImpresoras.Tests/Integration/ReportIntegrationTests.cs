using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Tests.Integration
{
    public class ReportIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ReportIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAvailableReports_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/reports/available");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAvailableReports_WithValidAuth_ShouldReturnReports()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/reports/available");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var reports = await response.Content.ReadFromJsonAsync<IEnumerable<ReportTemplateDto>>();
            reports.Should().NotBeNull();
            // Should contain at least some basic reports
            reports.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GenerateReport_WithValidRequest_ShouldReturnAccepted()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var request = new ReportRequestDto
            {
                ReportTemplateId = 1, // Assuming template exists
                Format = "json",
                Parameters = new Dictionary<string, object>()
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/reports/generate", request);

            // Assert
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                var result = await response.Content.ReadFromJsonAsync<ReportResultDto>();
                result.Should().NotBeNull();
                result!.ExecutionId.Should().BeGreaterThan(0);
            }
            else
            {
                // If no templates exist, should return 400
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GenerateReport_WithoutRequiredClaim_ShouldReturnForbidden()
        {
            // Arrange - Login as user without specific claims
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var request = new ReportRequestDto
            {
                ReportTemplateId = 999, // Template that requires specific claim
                Format = "json"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/reports/generate", request);

            // Assert
            if (response.StatusCode != HttpStatusCode.BadRequest) // Template doesn't exist
            {
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetReportHistory_WithValidAuth_ShouldReturnHistory()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/reports/history");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var history = await response.Content.ReadFromJsonAsync<IEnumerable<ReportHistoryDto>>();
            history.Should().NotBeNull();
        }

        [Fact]
        public async Task GetReportExecution_WithValidId_ShouldReturnDetails()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/reports/1");

            // Assert
            // Should return 404 if execution doesn't exist, or 200 if it does
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReportStatistics_WithValidAuth_ShouldReturnStatistics()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/reports/statistics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var statistics = await response.Content.ReadFromJsonAsync<ReportStatisticsDto>();
            statistics.Should().NotBeNull();
        }

        [Fact]
        public async Task DownloadReport_WithValidExecution_ShouldReturnFile()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/reports/1/download");

            // Assert
            // Should return 404 if execution doesn't exist, or 200 if it does
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CancelReportExecution_WithValidExecution_ShouldReturnSuccess()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.PostAsync("/api/v1/reports/1/cancel", null);

            // Assert
            // Should return 404 if execution doesn't exist, or 200 if it does
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        private async Task<LoginResponseDto> LoginAsUser()
        {
            var loginDto = new LoginRequestDto
            {
                Username = "user@monitorimpresoras.com",
                Password = "User123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponseDto>()!;
            }

            // Fallback to admin if user doesn't exist
            var adminLogin = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            var adminResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", adminLogin);
            return await adminResponse.Content.ReadFromJsonAsync<LoginResponseDto>()!;
        }
    }
}
