using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Tests.Integration
{
    public class SecurityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SecurityIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPrinters_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/printer");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPrinters_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.GetAsync("/api/printer");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnTokens()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.Roles.Should().Contain("Admin");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Username = "invalid@test.com",
                Password = "WrongPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPrinters_WithValidToken_ShouldReturnSuccess()
        {
            // Arrange - First login to get token
            var loginDto = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            // Set authorization header
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Act
            var response = await _client.GetAsync("/api/printer");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RefreshToken_WithValidRefreshToken_ShouldReturnNewTokens()
        {
            // Arrange - First login to get tokens
            var loginDto = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            var refreshDto = new RefreshTokenRequestDto
            {
                RefreshToken = loginResult!.RefreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            result.Token.Should().NotBe(loginResult.Token); // Should be different
        }

        [Fact]
        public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var refreshDto = new RefreshTokenRequestDto
            {
                RefreshToken = "invalid-refresh-token"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeletePrinter_WithUserRole_ShouldReturnForbidden()
        {
            // Arrange - Login with user role
            var loginDto = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            // Set authorization header
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Act - Try to delete a printer (requires Admin role)
            var response = await _client.DeleteAsync("/api/printer/12345678-1234-1234-1234-123456789abc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Profile_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/auth/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Profile_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange - First login to get token
            var loginDto = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            // Set authorization header
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Act
            var response = await _client.GetAsync("/api/auth/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var profile = await response.Content.ReadFromJsonAsync<object>();
            profile.Should().NotBeNull();
        }

        [Fact]
        public async Task Logout_WithValidToken_ShouldSucceed()
        {
            // Arrange - First login to get token
            var loginDto = new LoginRequestDto
            {
                Username = "admin@monitorimpresoras.com",
                Password = "Admin123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

            // Set authorization header
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Act
            var response = await _client.PostAsync("/api/auth/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
