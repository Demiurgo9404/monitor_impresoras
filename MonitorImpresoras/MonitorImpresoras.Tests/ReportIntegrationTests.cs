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
    public class ReportIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReportIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Fact]
        public async Task ReportFlow_GeneratePdfReport_ShouldSucceed()
        {
            // Arrange - Login como Admin
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Obtener un proyecto existente
            var projectsResponse = await _client.GetAsync("/api/project/my-projects");
            Assert.Equal(HttpStatusCode.OK, projectsResponse.StatusCode);

            var projectsJson = await projectsResponse.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<ProjectDto>>(projectsJson, _jsonOptions);
            
            if (!projects.Any())
            {
                // Crear un proyecto de prueba si no existe
                var createProjectDto = new CreateProjectDto
                {
                    Name = "Proyecto Test Reportes",
                    CompanyId = await GetFirstCompanyIdAsync(),
                    ClientName = "Cliente Test",
                    MonitoringIntervalMinutes = 5
                };

                var createProjectContent = new StringContent(JsonSerializer.Serialize(createProjectDto, _jsonOptions), Encoding.UTF8, "application/json");
                var createProjectResponse = await _client.PostAsync("/api/project", createProjectContent);
                Assert.Equal(HttpStatusCode.Created, createProjectResponse.StatusCode);

                var projectJson = await createProjectResponse.Content.ReadAsStringAsync();
                var project = JsonSerializer.Deserialize<ProjectDto>(projectJson, _jsonOptions);
                projects = new List<ProjectDto> { project };
            }

            var testProject = projects.First();

            // Act - Generar reporte PDF
            var generateReportDto = new GenerateReportDto
            {
                ProjectId = testProject.Id,
                ReportType = "Monthly",
                PeriodStart = DateTime.UtcNow.AddMonths(-1),
                PeriodEnd = DateTime.UtcNow,
                Format = "PDF",
                Title = "Reporte Test PDF",
                SendByEmail = false
            };

            var reportContent = new StringContent(JsonSerializer.Serialize(generateReportDto, _jsonOptions), Encoding.UTF8, "application/json");
            var generateResponse = await _client.PostAsync("/api/report/generate", reportContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, generateResponse.StatusCode);

            var reportJson = await generateResponse.Content.ReadAsStringAsync();
            var report = JsonSerializer.Deserialize<ReportDto>(reportJson, _jsonOptions);
            
            Assert.NotNull(report);
            Assert.Equal("PDF", report.FileFormat);
            Assert.Equal("Generated", report.Status);
            Assert.True(report.FileSizeBytes > 0);
        }

        [Fact]
        public async Task ReportFlow_GenerateExcelReport_ShouldSucceed()
        {
            // Arrange
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var projectId = await GetFirstProjectIdAsync();

            // Act - Generar reporte Excel
            var generateReportDto = new GenerateReportDto
            {
                ProjectId = projectId,
                ReportType = "Weekly",
                PeriodStart = DateTime.UtcNow.AddDays(-7),
                PeriodEnd = DateTime.UtcNow,
                Format = "Excel",
                Title = "Reporte Test Excel"
            };

            var reportContent = new StringContent(JsonSerializer.Serialize(generateReportDto, _jsonOptions), Encoding.UTF8, "application/json");
            var generateResponse = await _client.PostAsync("/api/report/generate", reportContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, generateResponse.StatusCode);

            var reportJson = await generateResponse.Content.ReadAsStringAsync();
            var report = JsonSerializer.Deserialize<ReportDto>(reportJson, _jsonOptions);
            
            Assert.NotNull(report);
            Assert.Equal("Excel", report.FileFormat);
            Assert.Equal("Generated", report.Status);
        }

        [Fact]
        public async Task ReportFlow_DownloadReport_ShouldReturnFile()
        {
            // Arrange
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Generar reporte primero
            var projectId = await GetFirstProjectIdAsync();
            var generateReportDto = new GenerateReportDto
            {
                ProjectId = projectId,
                ReportType = "Daily",
                PeriodStart = DateTime.UtcNow.AddDays(-1),
                PeriodEnd = DateTime.UtcNow,
                Format = "PDF",
                Title = "Reporte Test Descarga"
            };

            var reportContent = new StringContent(JsonSerializer.Serialize(generateReportDto, _jsonOptions), Encoding.UTF8, "application/json");
            var generateResponse = await _client.PostAsync("/api/report/generate", reportContent);
            
            var reportJson = await generateResponse.Content.ReadAsStringAsync();
            var report = JsonSerializer.Deserialize<ReportDto>(reportJson, _jsonOptions);

            // Act - Descargar reporte
            var downloadResponse = await _client.GetAsync($"/api/report/{report.Id}/download");

            // Assert
            Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
            Assert.Equal("application/pdf", downloadResponse.Content.Headers.ContentType?.MediaType);
            
            var fileContent = await downloadResponse.Content.ReadAsByteArrayAsync();
            Assert.True(fileContent.Length > 0);
        }

        [Fact]
        public async Task ReportFlow_GetReportsList_ShouldReturnPaginatedResults()
        {
            // Arrange
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/report?pageNumber=1&pageSize=5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var reportList = JsonSerializer.Deserialize<ReportListDto>(json, _jsonOptions);
            
            Assert.NotNull(reportList);
            Assert.True(reportList.PageNumber == 1);
            Assert.True(reportList.PageSize == 5);
        }

        [Fact]
        public async Task ReportFlow_QuickReport_ShouldGenerateAndDownload()
        {
            // Arrange
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var projectId = await GetFirstProjectIdAsync();

            // Act - Generar reporte rápido
            var response = await _client.GetAsync($"/api/report/quick/{projectId}?reportType=Monthly");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
            
            var fileContent = await response.Content.ReadAsByteArrayAsync();
            Assert.True(fileContent.Length > 0);
        }

        [Fact]
        public async Task ScheduledReportFlow_CreateAndManage_ShouldSucceed()
        {
            // Arrange
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var projectId = await GetFirstProjectIdAsync();

            // Act - Crear reporte programado
            var createScheduledDto = new CreateScheduledReportDto
            {
                ProjectId = projectId,
                Name = "Reporte Mensual Automático",
                ReportType = "Monthly",
                Schedule = "0 0 1 * *", // Primer día de cada mes
                Format = "PDF",
                EmailRecipients = new[] { "test@example.com" }
            };

            var content = new StringContent(JsonSerializer.Serialize(createScheduledDto, _jsonOptions), Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/scheduledreport", content);

            // Assert - Creación exitosa
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var scheduledJson = await createResponse.Content.ReadAsStringAsync();
            var scheduledReport = JsonSerializer.Deserialize<ScheduledReportDto>(scheduledJson, _jsonOptions);
            
            Assert.NotNull(scheduledReport);
            Assert.Equal("Reporte Mensual Automático", scheduledReport.Name);
            Assert.True(scheduledReport.IsActive);

            // Act - Obtener lista de reportes programados
            var listResponse = await _client.GetAsync("/api/scheduledreport");
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

            // Act - Ejecutar manualmente
            var executeResponse = await _client.PostAsync($"/api/scheduledreport/{scheduledReport.Id}/execute", null);
            Assert.Equal(HttpStatusCode.OK, executeResponse.StatusCode);

            // Act - Desactivar
            var toggleContent = new StringContent("false", Encoding.UTF8, "application/json");
            var toggleResponse = await _client.PatchAsync($"/api/scheduledreport/{scheduledReport.Id}/toggle", toggleContent);
            Assert.Equal(HttpStatusCode.OK, toggleResponse.StatusCode);
        }

        [Fact]
        public async Task ReportFlow_GetReportTypes_ShouldReturnAvailableOptions()
        {
            // Arrange
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/report/types");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var types = JsonSerializer.Deserialize<JsonElement>(json);
            
            Assert.True(types.TryGetProperty("reportTypes", out var reportTypes));
            Assert.True(types.TryGetProperty("formats", out var formats));
            Assert.True(types.TryGetProperty("statuses", out var statuses));
        }

        [Fact]
        public async Task ReportFlow_GetStats_ShouldReturnStatistics()
        {
            // Arrange
            var token = await LoginAsAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/report/stats");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<JsonElement>(json);
            
            Assert.True(stats.TryGetProperty("totalReports", out _));
            Assert.True(stats.TryGetProperty("reportsByType", out _));
            Assert.True(stats.TryGetProperty("reportsByStatus", out _));
        }

        [Fact]
        public async Task ReportFlow_ViewerRole_ShouldOnlyAllowRead()
        {
            // Arrange - Login como Viewer
            var token = await LoginAsViewerAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act - Intentar generar reporte (debe fallar)
            var generateReportDto = new GenerateReportDto
            {
                ProjectId = await GetFirstProjectIdAsync(),
                ReportType = "Monthly",
                PeriodStart = DateTime.UtcNow.AddMonths(-1),
                PeriodEnd = DateTime.UtcNow,
                Format = "PDF",
                Title = "Reporte No Autorizado"
            };

            var reportContent = new StringContent(JsonSerializer.Serialize(generateReportDto, _jsonOptions), Encoding.UTF8, "application/json");
            var generateResponse = await _client.PostAsync("/api/report/generate", reportContent);

            // Assert - Debe fallar con 403 Forbidden
            Assert.Equal(HttpStatusCode.Forbidden, generateResponse.StatusCode);

            // Act - Obtener lista de reportes (debe funcionar)
            var listResponse = await _client.GetAsync("/api/report");
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        }

        private async Task<string> LoginAsAdminAsync()
        {
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
            
            return authResponse.Token;
        }

        private async Task<string> LoginAsViewerAsync()
        {
            var loginDto = new LoginDto
            {
                Email = "user@demo.com",
                Password = "Password123!"
            };

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("X-Tenant-Id", "demo");

            var loginContent = new StringContent(JsonSerializer.Serialize(loginDto, _jsonOptions), Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(loginResult, _jsonOptions);
            
            return authResponse.Token;
        }

        private async Task<Guid> GetFirstCompanyIdAsync()
        {
            var companiesResponse = await _client.GetAsync("/api/company");
            var companiesJson = await companiesResponse.Content.ReadAsStringAsync();
            var companiesList = JsonSerializer.Deserialize<CompanyListDto>(companiesJson, _jsonOptions);
            
            return companiesList.Companies.First().Id;
        }

        private async Task<Guid> GetFirstProjectIdAsync()
        {
            var projectsResponse = await _client.GetAsync("/api/project/my-projects");
            var projectsJson = await projectsResponse.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<ProjectDto>>(projectsJson, _jsonOptions);
            
            if (!projects.Any())
            {
                // Crear proyecto si no existe
                var createProjectDto = new CreateProjectDto
                {
                    Name = "Proyecto Test",
                    CompanyId = await GetFirstCompanyIdAsync(),
                    ClientName = "Cliente Test"
                };

                var content = new StringContent(JsonSerializer.Serialize(createProjectDto, _jsonOptions), Encoding.UTF8, "application/json");
                var createResponse = await _client.PostAsync("/api/project", content);
                
                var projectJson = await createResponse.Content.ReadAsStringAsync();
                var project = JsonSerializer.Deserialize<ProjectDto>(projectJson, _jsonOptions);
                
                return project.Id;
            }
            
            return projects.First().Id;
        }
    }
}
