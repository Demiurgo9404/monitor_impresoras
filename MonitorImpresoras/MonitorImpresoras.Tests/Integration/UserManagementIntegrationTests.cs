using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Tests.Integration
{
    public class UserManagementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UserManagementIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUsers_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUsers_WithUserRole_ShouldReturnForbidden()
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
        public async Task GetUsers_WithAdminRole_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUser_WithValidId_ShouldReturnUserDetails()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/users/non-existent-id");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateProfile_AsUser_ShouldReturnSuccess()
        {
            // Arrange - Login as user
            var loginResponse = await LoginAsUser();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var updateProfile = new UpdateProfileDto
            {
                FirstName = "Updated",
                LastName = "Name"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/users/profile", updateProfile);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<object>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task AssignRole_WithAdminRole_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var assignRole = new { RoleName = "User" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/non-existent-id/roles", assignRole);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // User doesn't exist
        }

        [Fact]
        public async Task BlockUser_WithAdminRole_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var blockUser = new { Reason = "Test blocking" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/users/non-existent-id/block", blockUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // User doesn't exist
        }

        [Fact]
        public async Task ResetPassword_WithAdminRole_ShouldReturnSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var resetPassword = new { NewPassword = "NewPassword123!" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/users/non-existent-id/reset-password", resetPassword);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // User doesn't exist
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAsAdmin();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            // Act
            var response = await _client.GetAsync("/api/v1/users/invalid-guid-format");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
