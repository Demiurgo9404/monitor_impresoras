using System.Threading.Tasks;
using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface ITenantServiceSimple
    {
        Task<IEnumerable<TenantInfo>> GetAllTenantsAsync();
        Task<TenantInfo> GetTenantByKeyAsync(string tenantKey);
        Task<TenantInfo> GetTenantByIdAsync(Guid tenantId);
        Task CreateTenantAsync(CreateTenantDto dto);
        Task ProvisionTenantDatabaseAsync(TenantInfo tenant);
    }
}
