using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de empresas multi-tenant
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(
            ApplicationDbContext context,
            ITenantAccessor tenantAccessor,
            ILogger<CompanyService> logger)
        {
            _context = context;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        public async Task<CompanyListDto> GetCompaniesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var query = _context.Companies.AsQueryable();

                // Aplicar filtro de búsqueda si se proporciona
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(c => 
                        c.Name.Contains(searchTerm) ||
                        c.TaxId.Contains(searchTerm) ||
                        c.City.Contains(searchTerm) ||
                        c.ContactPerson.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();

                var companies = await query
                    .OrderBy(c => c.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CompanyDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        TaxId = c.TaxId,
                        Address = c.Address,
                        City = c.City,
                        State = c.State,
                        PostalCode = c.PostalCode,
                        Country = c.Country,
                        Phone = c.Phone,
                        Email = c.Email,
                        ContactPerson = c.ContactPerson,
                        IsActive = c.IsActive,
                        ContractStartDate = c.ContractStartDate,
                        ContractEndDate = c.ContractEndDate,
                        SubscriptionPlan = c.SubscriptionPlan,
                        MaxPrinters = c.MaxPrinters,
                        MaxProjects = c.MaxProjects,
                        MaxUsers = c.MaxUsers,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        CurrentPrinters = c.Printers.Count,
                        CurrentProjects = c.Projects.Count,
                        CurrentUsers = c.Users.Count
                    })
                    .ToListAsync();

                return new CompanyListDto
                {
                    Companies = companies,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving companies");
                throw;
            }
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(Guid id)
        {
            try
            {
                var company = await _context.Companies
                    .Include(c => c.Printers)
                    .Include(c => c.Projects)
                    .Include(c => c.Users)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (company == null)
                {
                    return null;
                }

                return new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    TaxId = company.TaxId,
                    Address = company.Address,
                    City = company.City,
                    State = company.State,
                    PostalCode = company.PostalCode,
                    Country = company.Country,
                    Phone = company.Phone,
                    Email = company.Email,
                    ContactPerson = company.ContactPerson,
                    IsActive = company.IsActive,
                    ContractStartDate = company.ContractStartDate,
                    ContractEndDate = company.ContractEndDate,
                    SubscriptionPlan = company.SubscriptionPlan,
                    MaxPrinters = company.MaxPrinters,
                    MaxProjects = company.MaxProjects,
                    MaxUsers = company.MaxUsers,
                    CreatedAt = company.CreatedAt,
                    UpdatedAt = company.UpdatedAt,
                    CurrentPrinters = company.Printers.Count,
                    CurrentProjects = company.Projects.Count,
                    CurrentUsers = company.Users.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company {CompanyId}", id);
                throw;
            }
        }

        public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto createDto)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new UnauthorizedAccessException("No tenant context available");
                }

                var company = new Company
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = createDto.Name,
                    TaxId = createDto.TaxId,
                    Address = createDto.Address,
                    City = createDto.City,
                    State = createDto.State,
                    PostalCode = createDto.PostalCode,
                    Country = createDto.Country,
                    Phone = createDto.Phone,
                    Email = createDto.Email,
                    ContactPerson = createDto.ContactPerson,
                    IsActive = true,
                    ContractStartDate = createDto.ContractStartDate,
                    ContractEndDate = createDto.ContractEndDate,
                    SubscriptionPlan = createDto.SubscriptionPlan,
                    MaxPrinters = createDto.MaxPrinters,
                    MaxProjects = createDto.MaxProjects,
                    MaxUsers = createDto.MaxUsers,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Company created successfully: {CompanyId} in tenant {TenantId}", company.Id, tenantId);

                return new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    TaxId = company.TaxId,
                    Address = company.Address,
                    City = company.City,
                    State = company.State,
                    PostalCode = company.PostalCode,
                    Country = company.Country,
                    Phone = company.Phone,
                    Email = company.Email,
                    ContactPerson = company.ContactPerson,
                    IsActive = company.IsActive,
                    ContractStartDate = company.ContractStartDate,
                    ContractEndDate = company.ContractEndDate,
                    SubscriptionPlan = company.SubscriptionPlan,
                    MaxPrinters = company.MaxPrinters,
                    MaxProjects = company.MaxProjects,
                    MaxUsers = company.MaxUsers,
                    CreatedAt = company.CreatedAt,
                    UpdatedAt = company.UpdatedAt,
                    CurrentPrinters = 0,
                    CurrentProjects = 0,
                    CurrentUsers = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                throw;
            }
        }

        public async Task<CompanyDto?> UpdateCompanyAsync(Guid id, UpdateCompanyDto updateDto)
        {
            try
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
                if (company == null)
                {
                    return null;
                }

                // Actualizar propiedades
                company.Name = updateDto.Name;
                company.TaxId = updateDto.TaxId;
                company.Address = updateDto.Address;
                company.City = updateDto.City;
                company.State = updateDto.State;
                company.PostalCode = updateDto.PostalCode;
                company.Country = updateDto.Country;
                company.Phone = updateDto.Phone;
                company.Email = updateDto.Email;
                company.ContactPerson = updateDto.ContactPerson;
                company.IsActive = updateDto.IsActive;
                company.ContractStartDate = updateDto.ContractStartDate;
                company.ContractEndDate = updateDto.ContractEndDate;
                company.SubscriptionPlan = updateDto.SubscriptionPlan;
                company.MaxPrinters = updateDto.MaxPrinters;
                company.MaxProjects = updateDto.MaxProjects;
                company.MaxUsers = updateDto.MaxUsers;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Company updated successfully: {CompanyId}", id);

                return await GetCompanyByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCompanyAsync(Guid id)
        {
            try
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
                if (company == null)
                {
                    return false;
                }

                // Soft delete
                company.IsActive = false;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Company soft deleted: {CompanyId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {CompanyId}", id);
                throw;
            }
        }

        public async Task<CompanyStatsDto?> GetCompanyStatsAsync(Guid id)
        {
            try
            {
                var company = await _context.Companies
                    .Include(c => c.Printers)
                    .Include(c => c.Projects)
                    .Include(c => c.Users)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (company == null)
                {
                    return null;
                }

                return new CompanyStatsDto
                {
                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    TotalPrinters = company.Printers.Count,
                    ActivePrinters = company.Printers.Count(p => p.IsOnline),
                    InactivePrinters = company.Printers.Count(p => !p.IsOnline),
                    TotalProjects = company.Projects.Count,
                    ActiveProjects = company.Projects.Count(p => p.IsActive),
                    TotalUsers = company.Users.Count,
                    ActiveUsers = company.Users.Count(u => u.IsActive),
                    MaxPrinters = company.MaxPrinters,
                    MaxProjects = company.MaxProjects,
                    MaxUsers = company.MaxUsers,
                    ContractStartDate = company.ContractStartDate,
                    ContractEndDate = company.ContractEndDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company stats {CompanyId}", id);
                throw;
            }
        }

        public async Task<bool> HasAccessToCompanyAsync(Guid companyId, string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return false;
                }

                // SuperAdmin tiene acceso a todo
                if (user.Role == QopiqRoles.SuperAdmin)
                {
                    return true;
                }

                // Verificar si el usuario pertenece a la empresa
                return user.CompanyId == companyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking company access for user {UserId} and company {CompanyId}", userId, companyId);
                return false;
            }
        }

        public async Task<List<CompanyDto>> GetUserCompaniesAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return new List<CompanyDto>();
                }

                IQueryable<Company> query = _context.Companies;

                // Si no es SuperAdmin, filtrar por empresa del usuario
                if (user.Role != QopiqRoles.SuperAdmin && user.CompanyId.HasValue)
                {
                    query = query.Where(c => c.Id == user.CompanyId.Value);
                }

                var companies = await query
                    .Include(c => c.Printers)
                    .Include(c => c.Projects)
                    .Include(c => c.Users)
                    .Select(c => new CompanyDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        TaxId = c.TaxId,
                        Address = c.Address,
                        City = c.City,
                        State = c.State,
                        PostalCode = c.PostalCode,
                        Country = c.Country,
                        Phone = c.Phone,
                        Email = c.Email,
                        ContactPerson = c.ContactPerson,
                        IsActive = c.IsActive,
                        ContractStartDate = c.ContractStartDate,
                        ContractEndDate = c.ContractEndDate,
                        SubscriptionPlan = c.SubscriptionPlan,
                        MaxPrinters = c.MaxPrinters,
                        MaxProjects = c.MaxProjects,
                        MaxUsers = c.MaxUsers,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        CurrentPrinters = c.Printers.Count,
                        CurrentProjects = c.Projects.Count,
                        CurrentUsers = c.Users.Count
                    })
                    .ToListAsync();

                return companies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user companies for user {UserId}", userId);
                throw;
            }
        }
    }
}
