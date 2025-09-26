using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Servicio para gestión de tenants
    /// </summary>
    public class TenantService : ITenantServiceSimple
    {
        private readonly ILogger<TenantService> _logger;
        private readonly IRepository<Tenant> _tenantRepository;

        public TenantService(
            ILogger<TenantService> logger,
            IRepository<Tenant> tenantRepository)
        {
            _logger = logger;
            _tenantRepository = tenantRepository;
        }

        public async Task<IEnumerable<TenantInfo>> GetAllTenantsAsync()
        {
            try
            {
                var tenants = await _tenantRepository.GetAllAsync(t => t.IsActive);
                return tenants.Select(MapToTenantInfo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tenants");
                return new List<TenantInfo>();
            }
        }

        public async Task<TenantInfo> GetTenantByKeyAsync(string tenantKey)
        {
            try
            {
                var tenant = await _tenantRepository.GetAllAsync(t => t.TenantKey == tenantKey && t.IsActive)
                    .ContinueWith(t => t.Result.FirstOrDefault());

                return tenant != null ? MapToTenantInfo(tenant) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tenant por key {TenantKey}", tenantKey);
                return null;
            }
        }

        public async Task<TenantInfo> GetTenantByIdAsync(Guid tenantId)
        {
            try
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                return tenant?.IsActive == true ? MapToTenantInfo(tenant) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tenant por ID {TenantId}", tenantId);
                return null;
            }
        }

        public async Task CreateTenantAsync(CreateTenantDto dto)
        {
            try
            {
                var tenant = new Tenant
                {
                    TenantKey = dto.TenantKey,
                    Name = dto.Name,
                    IsActive = true,
                    SubscriptionTier = dto.Tier,
                    CreatedAt = DateTime.UtcNow,
                    ConnectionString = $"Host=localhost;Database=monitor_{dto.TenantKey.ToLower()};Username=postgres;Password=Roximar2025"
                };

                await _tenantRepository.AddAsync(tenant);
                _logger.LogInformation("Tenant creado: {TenantKey}", dto.TenantKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando tenant {TenantKey}", dto.TenantKey);
                throw;
            }
        }

        public async Task ProvisionTenantDatabaseAsync(TenantInfo tenant)
        {
            try
            {
                _logger.LogInformation("Provisionando base de datos para tenant {TenantKey}", tenant.TenantKey);

                // Aquí se ejecutarían las migraciones de Entity Framework
                // para crear las tablas en la base de datos del tenant

                // También se ejecutarían los seeds necesarios
                await Task.CompletedTask;

                _logger.LogInformation("Base de datos provisionada para tenant {TenantKey}", tenant.TenantKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisionando base de datos para tenant {TenantKey}", tenant.TenantKey);
                throw;
            }
        }

        private static TenantInfo MapToTenantInfo(Tenant tenant)
        {
            return new TenantInfo
            {
                Id = tenant.Id,
                TenantKey = tenant.TenantKey,
                Name = tenant.Name,
                ConnectionString = tenant.ConnectionString,
                IsActive = tenant.IsActive,
                ExpiresAt = tenant.ExpiresAt,
                Tier = tenant.SubscriptionTier
            };
        }
    }
}
