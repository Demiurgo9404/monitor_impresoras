using System.Threading.Tasks;
using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Domain.Interfaces
{
    public interface ITenantDbContextFactory
    {
        Task<ITenantDbContext> CreateForTenantAsync(TenantInfo tenant);
    }
}
