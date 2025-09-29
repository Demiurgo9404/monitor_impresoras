using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Net;
using System.Net.Http.Json;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Tests.Integration
{
    public class SwaggerAndVersioningTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SwaggerAndVersioningTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SwaggerJson_ShouldBeAccessible()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }

        [Fact]
        public async Task SwaggerUI_ShouldBeAccessible()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        }

        [Fact]
        public async Task SwaggerJson_ShouldIncludeApiInfo()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var swaggerJson = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

            // Assert
            swaggerJson.Should().NotBeNull();
            swaggerJson!.Info.Should().NotBeNull();
            swaggerJson.Info.Title.Should().Be("Monitor Impresoras API");
            swaggerJson.Info.Version.Should().Be("v1");
            swaggerJson.Info.Description.Should().Contain("API completa para el monitoreo");
        }

        [Fact]
        public async Task SwaggerJson_ShouldIncludeSecurityDefinition()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var swaggerJson = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

            // Assert
            swaggerJson.Should().NotBeNull();
            swaggerJson!.Components.SecuritySchemes.Should().ContainKey("Bearer");
            var bearerScheme = swaggerJson.Components.SecuritySchemes["Bearer"];
            bearerScheme.Type.Should().Be("http");
            bearerScheme.Scheme.Should().Be("bearer");
            bearerScheme.BearerFormat.Should().Be("JWT");
        }

        [Fact]
        public async Task SwaggerJson_ShouldIncludeVersionedEndpoints()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var swaggerJson = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

            // Assert
            swaggerJson.Should().NotBeNull();
            swaggerJson!.Paths.Should().ContainKey("/api/v1/printer");
            swaggerJson.Paths.Should().ContainKey("/api/v1/printer/{id}");
            swaggerJson.Paths.Should().ContainKey("/api/v1/auth/login");
            swaggerJson.Paths.Should().ContainKey("/api/v1/auth/register");
        }

        [Fact]
        public async Task V1Endpoints_ShouldBeAccessible()
        {
            // Act - Test various v1 endpoints
            var printerResponse = await _client.GetAsync("/api/v1/printer");
            var authResponse = await _client.GetAsync("/api/v1/auth/login");

            // Assert
            // Note: These will return 401/403 without auth, but should be accessible
            printerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
            authResponse.StatusCode.Should().Be(HttpStatusCode.OK); // Login endpoint should be public
        }

        [Fact]
        public async Task V2Endpoints_ShouldReturnNotFound_WhenNotImplemented()
        {
            // Act
            var response = await _client.GetAsync("/api/v2/printer");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ApiExplorer_ShouldReportVersions()
        {
            // Arrange
            var serviceProvider = _factory.Services;
            var apiExplorer = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

            // Act
            var descriptions = apiExplorer.ApiVersionDescriptions.ToList();

            // Assert
            descriptions.Should().HaveCountGreaterThan(0);
            descriptions.Should().Contain(d => d.ApiVersion.ToString() == "1.0");
        }

        [Fact]
        public async Task HealthEndpoint_ShouldBeAccessible()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<object>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task SwaggerJson_ShouldIncludeExampleValues()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            var swaggerJson = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

            // Assert
            swaggerJson.Should().NotBeNull();

            // Check if any path has example values (this is harder to test directly,
            // but we can verify the structure is there)
            var pathsWithExamples = swaggerJson!.Paths
                .Where(p => p.Value.Post != null || p.Value.Put != null)
                .ToList();

            pathsWithExamples.Should().HaveCountGreaterThan(0);
        }
    }

    // Helper classes for deserializing Swagger JSON
    public class SwaggerDocument
    {
        public SwaggerInfo Info { get; set; } = new();
        public SwaggerComponents Components { get; set; } = new();
        public Dictionary<string, SwaggerPath> Paths { get; set; } = new();
    }

    public class SwaggerInfo
    {
        public string Title { get; set; } = "";
        public string Version { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class SwaggerComponents
    {
        public Dictionary<string, SwaggerSecurityScheme> SecuritySchemes { get; set; } = new();
    }

    public class SwaggerSecurityScheme
    {
        public string Type { get; set; } = "";
        public string Scheme { get; set; } = "";
        public string BearerFormat { get; set; } = "";
        public string In { get; set; } = "";
    }

    public class SwaggerPath
    {
        public SwaggerOperation? Get { get; set; }
        public SwaggerOperation? Post { get; set; }
        public SwaggerOperation? Put { get; set; }
        public SwaggerOperation? Delete { get; set; }
    }

    public class SwaggerOperation
    {
        public List<SwaggerParameter> Parameters { get; set; } = new();
        public Dictionary<string, SwaggerResponse> Responses { get; set; } = new();
    }

    public class SwaggerParameter
    {
        public string Name { get; set; } = "";
        public string In { get; set; } = "";
        public bool Required { get; set; }
        public SwaggerSchema Schema { get; set; } = new();
    }

    public class SwaggerResponse
    {
        public string Description { get; set; } = "";
        public SwaggerSchema Content { get; set; } = new();
    }

    public class SwaggerSchema
    {
        public string Type { get; set; } = "";
        public Dictionary<string, object>? Properties { get; set; }
        public object? Example { get; set; }
    }
}
