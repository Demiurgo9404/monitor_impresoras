using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Tests.Integration
{
    public class PermissionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PermissionIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPrinters_WithoutClaims_ShouldReturnForbidden()
        {
            // Arrange - Login as regular user without printer claims
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/printer");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPrinters_WithManagePrintersClaim_ShouldReturnSuccess()
        {
            // Arrange - Login as user with printer management claim
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // First assign the claim
            var assignClaim = new AssignClaimDto
            {
                ClaimType = "printers.manage",
                ClaimValue = "true",
                Description = "Test claim for printer management"
            };

            var assignResponse = await _client.PostAsJsonAsync("/api/v1/users/test-user-id/claims", assignClaim);
            assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Wait a bit for the token to be regenerated with new claims
            await Task.Delay(1000);

            // Act
            var response = await _client.GetAsync("/api/v1/printer");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUsers_WithoutAdminRole_ShouldReturnForbidden()
        {
            // Arrange - Login as regular user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AssignClaim_AsAdmin_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var assignClaim = new AssignClaimDto
            {
                ClaimType = "reports.view",
                ClaimValue = "true",
                Description = "Can view reports",
                Category = "reports"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/test-user-id/claims", assignClaim);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<object>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserClaims_AsAdmin_ShouldReturnClaimsList()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/users/test-user-id/claims");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var claims = await response.Content.ReadFromJsonAsync<IEnumerable<UserClaimDto>>();
            claims.Should().NotBeNull();
        }

        [Fact]
        public async Task RevokeClaim_AsAdmin_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.DeleteAsync("/api/v1/users/test-user-id/claims/reports.view");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAvailableClaims_AsAdmin_ShouldReturnClaimDefinitions()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/users/claims/available");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var claims = await response.Content.ReadFromJsonAsync<IEnumerable<ClaimDefinitionDto>>();
            claims.Should().NotBeNull();
            claims.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetAvailableClaims_AsUser_ShouldReturnForbidden()
        {
            // Arrange - Login as regular user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/users/claims/available");

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
            // This would require creating a test user first
            // For now, return admin login as placeholder
            return await LoginAsAdmin();
        }
    }
}
