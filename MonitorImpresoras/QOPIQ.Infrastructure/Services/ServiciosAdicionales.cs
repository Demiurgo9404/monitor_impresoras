// SERVICIOS FALTANTES PARA INFRASTRUCTURE

using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces;
using QOPIQ.Application.DTOs;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace QOPIQ.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;

        public AuthService(ILogger<AuthService> logger, IJwtService jwtService, ApplicationDbContext context)
        {
            _logger = logger;
            _jwtService = jwtService;
            _context = context;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null && user.IsActive)
                {
                    return await _jwtService.GenerateAccessTokenAsync(user);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", email);
                return string.Empty;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                return _jwtService.ValidateToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
                // Implementación básica - en producción usar SMTP real
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
            }
        }
    }

    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantService> _logger;

        public TenantService(ApplicationDbContext context, ILogger<TenantService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Tenant>> GetAllAsync()
        {
            try
            {
                return await _context.Tenants.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tenants");
                return new List<Tenant>();
            }
        }

        public async Task<Tenant?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Tenants.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant {Id}", id);
                return null;
            }
        }
    }

    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(ApplicationDbContext context, ILogger<CompanyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Company>> GetAllAsync(string tenantId)
        {
            try
            {
                return await _context.Companies
                    .Where(c => c.TenantId == tenantId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies for tenant {TenantId}", tenantId);
                return new List<Company>();
            }
        }

        public async Task<Company?> GetByIdAsync(Guid id, string tenantId)
        {
            try
            {
                return await _context.Companies
                    .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company {Id} for tenant {TenantId}", id, tenantId);
                return null;
            }
        }

        public async Task<Company> CreateAsync(Company company)
        {
            try
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
                return company;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                throw;
            }
        }

        public async Task UpdateAsync(Company company)
        {
            try
            {
                _context.Companies.Update(company);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {Id}", company.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, string tenantId)
        {
            try
            {
                var company = await GetByIdAsync(id, tenantId);
                if (company != null)
                {
                    _context.Companies.Remove(company);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {Id}", id);
                throw;
            }
        }
    }

    public class TenantAccessor : ITenantAccessor
    {
        private string? _tenantId;

        public string? TenantId => _tenantId;

        public void SetTenant(string tenantId)
        {
            _tenantId = tenantId;
        }
    }
}

