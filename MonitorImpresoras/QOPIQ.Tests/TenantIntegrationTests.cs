using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;

namespace QOPIQ.Tests
{
    public class TenantIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TenantIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task TenantHealth_WithValidTenant_ShouldReturn200()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

            // Act
            var response = await _client.GetAsync("/api/tenant/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            Assert.Equal("Healthy", result.GetProperty("status").GetString());
            Assert.Equal("demo", result.GetProperty("tenant").GetProperty("id").GetString());
        }

        [Fact]
        public async Task TenantHealth_WithInvalidTenant_ShouldReturn403()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "invalid-tenant");

            // Act
            var response = await _client.GetAsync("/api/tenant/health");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TenantHealth_WithoutTenantHeader_ShouldReturn403()
        {
            // Act
            var response = await _client.GetAsync("/api/tenant/health");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            Assert.Equal("Tenant header is required", result.GetProperty("error").GetString());
        }

        [Fact]
        public async Task TenantValidate_WithValidTenant_ShouldReturnTenantInfo()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "contoso");

            // Act
            var response = await _client.GetAsync("/api/tenant/validate");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            Assert.True(result.GetProperty("hasValidTenant").GetBoolean());
            Assert.Equal("contoso", result.GetProperty("tenantId").GetString());
        }

        [Fact]
        public async Task HealthEndpoint_ShouldBeExcludedFromTenantValidation()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            // No debería devolver 403, debería pasar sin validación de tenant
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SwaggerEndpoint_ShouldBeExcludedFromTenantValidation()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            // No debería devolver 403, debería pasar sin validación de tenant
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("demo")]
        [InlineData("contoso")]
        [InlineData("acme")]
        public async Task TenantInfo_WithValidTenants_ShouldReturnCorrectInfo(string tenantId)
        {
            // Arrange
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);

            // Act
            var response = await _client.GetAsync("/api/tenant/info");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            Assert.Equal(tenantId, result.GetProperty("tenantId").GetString());
            Assert.True(result.GetProperty("isActive").GetBoolean());
        }

        [Fact]
        public async Task ApiEndpoints_WithoutTenant_ShouldReturn403()
        {
            // Arrange - no tenant header

            // Act & Assert
            var endpoints = new[]
            {
                "/api/printers",
                "/api/tenant/info",
                "/api/tenant/health"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }
    }
}

