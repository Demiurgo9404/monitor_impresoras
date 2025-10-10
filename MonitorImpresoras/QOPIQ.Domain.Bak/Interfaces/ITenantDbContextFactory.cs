using System.Threading.Tasks;
using QOPIQ.Domain.DTOs;

namespace QOPIQ.Domain.Interfaces
{
    public interface ITenantDbContextFactory
    {
        Task<ITenantDbContext> CreateForTenantAsync(TenantInfo tenant);
    }
}

