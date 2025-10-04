using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación de ITenantResolver que obtiene el tenant del header HTTP
    /// </summary>
    public class TenantResolver : ITenantResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantResolver> _logger;

        private const string TENANT_HEADER = "X-Tenant-Id";

        public TenantResolver(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<TenantResolver> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
        }

        public string? GetCurrentTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext is null, cannot resolve tenant");
                return null;
            }

            // Intentar obtener del header
            if (httpContext.Request.Headers.TryGetValue(TENANT_HEADER, out var headerValue))
            {
                var tenantId = headerValue.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    _logger.LogDebug("Tenant resolved from header: {TenantId}", tenantId);
                    return tenantId.Trim();
                }
            }

            // Intentar obtener de query parameter (fallback para testing)
            if (httpContext.Request.Query.TryGetValue("tenant", out var queryValue))
            {
                var tenantId = queryValue.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    _logger.LogDebug("Tenant resolved from query: {TenantId}", tenantId);
                    return tenantId.Trim();
                }
            }

            _logger.LogWarning("No tenant found in request headers or query parameters");
            return null;
        }

        public async Task<bool> IsValidTenantAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return false;
            }

            try
            {
                var exists = await _context.Tenants
                    .Where(t => t.TenantKey == tenantId && t.IsActive)
                    .AnyAsync();

                _logger.LogDebug("Tenant {TenantId} validation result: {IsValid}", tenantId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<TenantInfo?> GetCurrentTenantAsync()
        {
            var tenantId = GetCurrentTenantId();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return null;
            }

            try
            {
                var tenant = await _context.Tenants
                    .Where(t => t.TenantKey == tenantId && t.IsActive)
                    .Select(t => new TenantInfo
                    {
                        TenantId = t.TenantKey,
                        Name = t.Name,
                        CompanyName = t.CompanyName,
                        IsActive = t.IsActive,
                        ExpiresAt = t.ExpiresAt
                    })
                    .FirstOrDefaultAsync();

                if (tenant != null)
                {
                    _logger.LogDebug("Tenant info retrieved: {TenantName}", tenant.Name);
                }
                else
                {
                    _logger.LogWarning("Tenant {TenantId} not found or inactive", tenantId);
                }

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant info for {TenantId}", tenantId);
                return null;
            }
        }
    }
}
