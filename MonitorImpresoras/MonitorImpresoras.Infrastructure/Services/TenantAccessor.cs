using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación de ITenantAccessor usando AsyncLocal para thread-safety
    /// </summary>
    public class TenantAccessor : ITenantAccessor
    {
        private static readonly AsyncLocal<string?> _tenantId = new();
        private static readonly AsyncLocal<TenantInfo?> _tenantInfo = new();

        public string? TenantId => _tenantId.Value;

        public TenantInfo? TenantInfo => _tenantInfo.Value;

        public bool HasTenant => !string.IsNullOrEmpty(_tenantId.Value);

        public void SetTenant(string tenantId, TenantInfo? tenantInfo = null)
        {
            _tenantId.Value = tenantId;
            _tenantInfo.Value = tenantInfo;
        }
    }
}
