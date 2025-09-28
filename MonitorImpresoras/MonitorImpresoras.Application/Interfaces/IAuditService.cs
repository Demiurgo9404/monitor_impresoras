using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string action, string entity, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
        Task<IEnumerable<AuditLog>> GetLogsAsync(string? userId = null, string? action = null, string? entity = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
