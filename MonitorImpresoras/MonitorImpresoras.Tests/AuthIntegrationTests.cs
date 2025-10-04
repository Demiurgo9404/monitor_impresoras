using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MonitorImpresoras.Application.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MonitorImpresoras.Tests
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Fact]
        public async Task AuthFlow_LoginAsAdmin_ShouldAllowCompanyCRUD()
        {
            // Arrange - Login como Admin
            var loginDto = new LoginDto
            {
                Email = "admin@demo.com",
                Password = "Password123!"
            };

            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

            // Act - Login
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert - Login exitoso
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(loginResult, _jsonOptions);
            
            Assert.NotNull(authResponse);
            Assert.NotEmpty(authResponse.Token);
            Assert.Equal(QopiqRoles.CompanyAdmin, authResponse.User.Role);

            // Configurar token para siguientes requests
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

            // Test - Crear empresa (debe funcionar para Admin)
            var createCompanyDto = new CreateCompanyDto
            {
                Name = "Test Company Integration",
                Email = "test@integration.com",
                ContactPerson = "Integration Test",
                MaxPrinters = 20,
                MaxUsers = 10,
                MaxProjects = 5
            };

            var companyContent = new StringContent(JsonSerializer.Serialize(createCompanyDto, _jsonOptions), Encoding.UTF8, "application/json");
            var createCompanyResponse = await _client.PostAsync("/api/company", companyContent);

            // Assert - Creación exitosa
            Assert.Equal(HttpStatusCode.Created, createCompanyResponse.StatusCode);

            var companyResult = await createCompanyResponse.Content.ReadAsStringAsync();
            var company = JsonSerializer.Deserialize<CompanyDto>(companyResult, _jsonOptions);
            
            Assert.NotNull(company);
            Assert.Equal("Test Company Integration", company.Name);

            // Test - Obtener empresas (debe funcionar)
            var getCompaniesResponse = await _client.GetAsync("/api/company");
            Assert.Equal(HttpStatusCode.OK, getCompaniesResponse.StatusCode);

            // Test - Crear proyecto
            var createProjectDto = new CreateProjectDto
            {
                Name = "Test Project Integration",
                CompanyId = company.Id,
                ClientName = "Integration Client",
                MonitoringIntervalMinutes = 10
            };

            var projectContent = new StringContent(JsonSerializer.Serialize(createProjectDto, _jsonOptions), Encoding.UTF8, "application/json");
            var createProjectResponse = await _client.PostAsync("/api/project", projectContent);

            // Assert - Creación de proyecto exitosa
            Assert.Equal(HttpStatusCode.Created, createProjectResponse.StatusCode);
        }

        [Fact]
        public async Task AuthFlow_LoginAsViewer_ShouldOnlyAllowRead()
        {
            // Arrange - Login como Viewer
            var loginDto = new LoginDto
            {
                Email = "user@demo.com",
                Password = "Password123!"
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

            // Act - Login
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert - Login exitoso
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(loginResult, _jsonOptions);
            
            Assert.NotNull(authResponse);
            Assert.Equal(QopiqRoles.Viewer, authResponse.User.Role);

            // Configurar token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

            // Test - Obtener empresas (debe funcionar para lectura)
            var getCompaniesResponse = await _client.GetAsync("/api/company");
            Assert.Equal(HttpStatusCode.OK, getCompaniesResponse.StatusCode);

            // Test - Crear empresa (debe fallar para Viewer)
            var createCompanyDto = new CreateCompanyDto
            {
                Name = "Unauthorized Company",
                Email = "unauthorized@test.com",
                ContactPerson = "Unauthorized User"
            };

            var companyContent = new StringContent(JsonSerializer.Serialize(createCompanyDto, _jsonOptions), Encoding.UTF8, "application/json");
            var createCompanyResponse = await _client.PostAsync("/api/company", companyContent);

            // Assert - Debe fallar con 403 Forbidden
            Assert.Equal(HttpStatusCode.Forbidden, createCompanyResponse.StatusCode);

            // Test - Crear proyecto (debe fallar para Viewer)
            var createProjectDto = new CreateProjectDto
            {
                Name = "Unauthorized Project",
                CompanyId = Guid.NewGuid(),
                ClientName = "Unauthorized Client"
            };

            var projectContent = new StringContent(JsonSerializer.Serialize(createProjectDto, _jsonOptions), Encoding.UTF8, "application/json");
            var createProjectResponse = await _client.PostAsync("/api/project", projectContent);

            // Assert - Debe fallar con 403 Forbidden
            Assert.Equal(HttpStatusCode.Forbidden, createProjectResponse.StatusCode);
        }

        [Fact]
        public async Task AuthFlow_InvalidTenant_ShouldReturn403()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "admin@demo.com",
                Password = "Password123!"
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "invalid-tenant");

            // Act
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
        }

        [Fact]
        public async Task AuthFlow_NoTenantHeader_ShouldReturn403()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "admin@demo.com",
                Password = "Password123!"
            };

            _client.DefaultRequestHeaders.Clear();
            // No agregar X-Tenant-Id header

            // Act
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
        }

        [Fact]
        public async Task AuthFlow_InvalidCredentials_ShouldReturn401()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "admin@demo.com",
                Password = "WrongPassword"
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

            // Act
            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }

        [Fact]
        public async Task AuthFlow_ExpiredToken_ShouldReturn401()
        {
            // Arrange - Token inválido/expirado
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

            // Act
            var response = await _client.GetAsync("/api/company");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuthFlow_TestEndpoints_ShouldReturnAuthInfo()
        {
            // Arrange - Login como Admin
            var loginDto = new LoginDto
            {
                Email = "admin@demo.com",
                Password = "Password123!"
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(loginResult, _jsonOptions);
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

            // Act - Test auth endpoints
            var authTestResponse = await _client.GetAsync("/api/auth/test");
            var companyTestResponse = await _client.GetAsync("/api/company/test-auth");
            var projectTestResponse = await _client.GetAsync("/api/project/test-auth");

            // Assert
            Assert.Equal(HttpStatusCode.OK, authTestResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, companyTestResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, projectTestResponse.StatusCode);

            // Verificar contenido de respuesta
            var authTestResult = await authTestResponse.Content.ReadAsStringAsync();
            var authTestData = JsonSerializer.Deserialize<JsonElement>(authTestResult);
            
            Assert.Equal("demo", authTestData.GetProperty("tenantId").GetString());
            Assert.Equal(QopiqRoles.CompanyAdmin, authTestData.GetProperty("role").GetString());
        }

        [Fact]
        public async Task AuthFlow_CrossTenantAccess_ShouldBeDenied()
        {
            // Arrange - Login en tenant demo
            var loginDto = new LoginDto
            {
                Email = "admin@demo.com",
                Password = "Password123!"
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(loginResult, _jsonOptions);
            
            // Act - Intentar acceder con token de demo pero header de contoso
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "contoso"); // Diferente tenant
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

            var response = await _client.GetAsync("/api/company");

            // Assert - Debe fallar porque el token es de demo pero el header dice contoso
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
