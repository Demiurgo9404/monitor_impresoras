using Blazored.LocalStorage;
using MonitorImpresoras.Web.Models;

namespace MonitorImpresoras.Web.Services
{
    public class TenantService
    {
        private readonly ApiService _apiService;
        private readonly ILocalStorageService _localStorage;

        public TenantService(ApiService apiService, ILocalStorageService localStorage)
        {
            _apiService = apiService;
            _localStorage = localStorage;
        }

        public async Task<List<TenantInfo>> GetAvailableTenantsAsync()
        {
            // En una implementación real, esto podría venir de un endpoint público
            // Por ahora, devolvemos tenants de demostración
            return new List<TenantInfo>
            {
                new TenantInfo { Id = "demo", Name = "Empresa Demo", Description = "Tenant de demostración" },
                new TenantInfo { Id = "acme", Name = "ACME Corp", Description = "Corporación ACME" },
                new TenantInfo { Id = "techsol", Name = "TechSolutions", Description = "Soluciones Tecnológicas" }
            };
        }

        public async Task<string?> GetCurrentTenantIdAsync()
        {
            return await _localStorage.GetItemAsync<string>("tenantId");
        }

        public async Task SetCurrentTenantAsync(string tenantId)
        {
            await _localStorage.SetItemAsync("tenantId", tenantId);
        }

        public async Task<CompanyInfo?> GetCurrentCompanyInfoAsync()
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId))
                return null;

            // Obtener información de la empresa actual
            return await _apiService.GetAsync<CompanyInfo>("/api/company/current");
        }

        public async Task<List<ProjectSummary>> GetTenantProjectsAsync()
        {
            return await _apiService.GetAsync<List<ProjectSummary>>("/api/project/my-projects") ?? new List<ProjectSummary>();
        }

        public async Task<DashboardStats?> GetTenantStatsAsync()
        {
            return await _apiService.GetAsync<DashboardStats>("/api/dashboard/stats");
        }
    }
}
