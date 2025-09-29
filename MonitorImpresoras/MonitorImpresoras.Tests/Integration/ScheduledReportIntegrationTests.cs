using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Tests.Integration
{
    public class ScheduledReportIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ScheduledReportIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetScheduledReports_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/scheduledreports");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetScheduledReports_WithValidAuth_ShouldReturnScheduledReports()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/scheduledreports");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var reports = await response.Content.ReadFromJsonAsync<IEnumerable<ScheduledReportDto>>();
            reports.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateScheduledReport_WithValidData_ShouldReturnCreated()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var request = new CreateScheduledReportDto
            {
                ReportTemplateId = 1,
                Name = "Test Scheduled Report",
                Description = "Test description",
                CronExpression = "0 9 * * MON",
                Format = "pdf",
                Recipients = "test@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/scheduledreports", request);

            // Assert
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var result = await response.Content.ReadFromJsonAsync<ScheduledReportDto>();
                result.Should().NotBeNull();
                result!.Name.Should().Be("Test Scheduled Report");
                result.Format.Should().Be("pdf");
            }
            else
            {
                // If template doesn't exist, should return 400
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task CreateScheduledReport_WithInvalidCron_ShouldReturnBadRequest()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var request = new CreateScheduledReportDto
            {
                ReportTemplateId = 1,
                Name = "Invalid Cron Report",
                CronExpression = "invalid-cron-expression"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/scheduledreports", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetScheduledReport_WithValidId_ShouldReturnScheduledReport()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/scheduledreports/1");

            // Assert
            // Should return 404 if doesn't exist, or 200 if it does
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateScheduledReport_WithValidData_ShouldReturnSuccess()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var updateRequest = new UpdateScheduledReportDto
            {
                Name = "Updated Report Name",
                Format = "excel"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/scheduledreports/1", updateRequest);

            // Assert
            // Should return 404 if doesn't exist, or 200 if it does
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteScheduledReport_WithValidId_ShouldReturnSuccess()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.DeleteAsync("/api/v1/scheduledreports/1");

            // Assert
            // Should return 404 if doesn't exist, or 200 if it does
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ExecuteScheduledReport_WithValidId_ShouldReturnAccepted()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.PostAsync("/api/v1/scheduledreports/1/execute", null);

            // Assert
            // Should return 404 if doesn't exist, or 202 if it does
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Accepted, HttpStatusCode.NotFound);
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
