using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para gestión de reportes
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReportTemplate>> GetAvailableReportTemplatesAsync(string userId) => await _context.ReportTemplates.AsNoTracking()
            .Include(rt => rt.CreatedByUser)
            .Where(rt => rt.IsActive && (
                rt.RequiredClaim == null ||
                _context.UserClaims.AsNoTracking().Where(uc => uc.UserId == userId && uc.IsValid).Select(uc => uc.ClaimType).Contains(rt.RequiredClaim) ||
                _context.UserRoles.AsNoTracking().Where(ur => ur.UserId == userId).Join(_context.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).Contains("Admin")
            ))
            .OrderBy(rt => rt.Category)
            .ThenBy(rt => rt.Name)
            .ToListAsync();

        public async Task<ReportTemplate?> GetReportTemplateByIdAsync(int templateId) => await _context.ReportTemplates.AsNoTracking()
            .Include(rt => rt.CreatedByUser)
            .FirstOrDefaultAsync(rt => rt.Id == templateId);

        public async Task<IEnumerable<ReportExecution>> GetUserReportExecutionsAsync(string userId, int page, int pageSize) => await _context.ReportExecutions.AsNoTracking()
            .Include(re => re.ReportTemplate)
            .Where(re => re.ExecutedByUserId == userId)
            .OrderByDescending(re => re.StartedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        public async Task<ReportExecution?> GetReportExecutionByIdAsync(int executionId)
        {
            return await _context.ReportExecutions
                .Include(re => re.ReportTemplate)
                .FirstOrDefaultAsync(re => re.Id == executionId);
        }

        public async Task<ReportExecution> CreateReportExecutionAsync(ReportExecution execution)
        {
            _context.ReportExecutions.Add(execution);
            await _context.SaveChangesAsync();
            return execution;
        }

        public async Task UpdateReportExecutionAsync(ReportExecution execution)
        {
            _context.ReportExecutions.Update(execution);
            await _context.SaveChangesAsync();
        }

        public async Task<ReportStatisticsDto> GetReportStatisticsAsync(string userId)
        {
            var executions = await _context.ReportExecutions.AsNoTracking()
                .Where(re => re.ExecutedByUserId == userId)
                .Select(re => new
                {
                    re.Status,
                    re.ExecutionTimeSeconds,
                    re.FileSize,
                    re.StartedAtUtc,
                    re.Format,
                    TemplateCategory = _context.ReportTemplates.AsNoTracking()
                        .Where(rt => rt.Id == re.ReportTemplateId)
                        .Select(rt => rt.Category)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();

            var totalExecutions = executions.Count;
            var successfulExecutions = executions.Count(e => e.Status == "completed");
            var failedExecutions = executions.Count(e => e.Status == "failed");
            var averageExecutionTime = executions
                .Where(e => e.ExecutionTimeSeconds.HasValue)
                .Average(e => e.ExecutionTimeSeconds.Value);
            var totalFileSize = executions.Sum(e => e.FileSize);
            var lastExecutionDate = executions
                .Where(e => e.StartedAtUtc != default)
                .Max(e => (DateTime?)e.StartedAtUtc);

            var executionsByFormat = executions
                .GroupBy(e => e.Format)
                .ToDictionary(g => g.Key, g => g.Count());

            var executionsByCategory = executions
                .GroupBy(e => e.TemplateCategory)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ReportStatisticsDto
            {
                TotalExecutions = totalExecutions,
                SuccessfulExecutions = successfulExecutions,
                FailedExecutions = failedExecutions,
                AverageExecutionTimeSeconds = averageExecutionTime,
                TotalFileSizeBytes = totalFileSize,
                LastExecutionDate = lastExecutionDate,
                ExecutionsByFormat = executionsByFormat,
                ExecutionsByCategory = executionsByCategory
            };
        }

        public async Task<IEnumerable<object>> GetPrinterReportDataAsync(ReportFilterDto? filters = null)
        {
            var query = _context.Printers.AsNoTracking().AsQueryable();

            if (filters?.DateFrom.HasValue == true)
            {
                query = query.Where(p => p.CreatedAt >= filters.DateFrom.Value);
            }

            if (filters?.DateTo.HasValue == true)
            {
                query = query.Where(p => p.CreatedAt <= filters.DateTo.Value);
            }

            if (filters?.FieldFilters?.ContainsKey("Status") == true)
            {
                if (filters.FieldFilters["Status"] is string status)
                {
                    query = query.Where(p => p.Status == status);
                }
            }

            if (filters?.FieldFilters?.ContainsKey("Brand") == true)
            {
                if (filters.FieldFilters["Brand"] is string brand)
                {
                    query = query.Where(p => p.Brand.Contains(brand));
                }
            }

            if (filters?.MaxRecords.HasValue == true)
            {
                query = query.Take(filters.MaxRecords.Value);
            }

            var data = await query
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand,
                    Model = p.Model,
                    Status = p.Status,
                    Location = p.Location,
                    IpAddress = p.IpAddress,
                    SerialNumber = p.SerialNumber,
                    CreatedAt = p.CreatedAt,
                    LastMaintenanceDate = p.LastMaintenanceDate,
                    TonerLevel = p.TonerLevel,
                    PaperLevel = p.PaperLevel
                })
                .ToListAsync();

            return data.Cast<object>();
        }

        public async Task<IEnumerable<object>> GetUserReportDataAsync(ReportFilterDto? filters = null)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (filters?.DateFrom.HasValue == true)
            {
                query = query.Where(u => u.CreatedAtUtc >= filters.DateFrom.Value);
            }

            if (filters?.DateTo.HasValue == true)
            {
                query = query.Where(u => u.CreatedAtUtc <= filters.DateTo.Value);
            }

            if (filters?.FieldFilters?.ContainsKey("Department") == true)
            {
                if (filters.FieldFilters["Department"] is string department)
                {
                    query = query.Where(u => u.Department != null && u.Department.Contains(department));
                }
            }

            if (filters?.FieldFilters?.ContainsKey("IsActive") == true)
            {
                if (filters.FieldFilters["IsActive"] is bool isActive)
                {
                    query = query.Where(u => u.IsActive == isActive);
                }
            }

            if (filters?.MaxRecords.HasValue == true)
            {
                query = query.Take(filters.MaxRecords.Value);
            }

            var data = await query
                .Select(u => new
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Department = u.Department,
                    IsActive = u.IsActive,
                    LastLoginAtUtc = u.LastLoginAtUtc,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    CreatedAtUtc = u.CreatedAtUtc
                })
                .ToListAsync();

            return data.Cast<object>();
        }

        public async Task<IEnumerable<object>> GetAuditReportDataAsync(ReportFilterDto? filters = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (filters?.DateFrom.HasValue == true)
            {
                query = query.Where(a => a.Timestamp >= filters.DateFrom.Value);
            }

            if (filters?.DateTo.HasValue == true)
            {
                query = query.Where(a => a.Timestamp <= filters.DateTo.Value);
            }

            if (filters?.FieldFilters?.ContainsKey("Action") == true)
            {
                if (filters.FieldFilters["Action"] is string action)
                {
                    query = query.Where(a => a.Action.Contains(action));
                }
            }

            if (filters?.FieldFilters?.ContainsKey("EntityType") == true)
            {
                if (filters.FieldFilters["EntityType"] is string entityType)
                {
                    query = query.Where(a => a.EntityType.Contains(entityType));
                }
            }

            if (filters?.FieldFilters?.ContainsKey("UserId") == true)
            {
                if (filters.FieldFilters["UserId"] is string userId)
                {
                    query = query.Where(a => a.UserId.Contains(userId));
                }
            }

            if (filters?.MaxRecords.HasValue == true)
            {
                query = query.Take(filters.MaxRecords.Value);
            }

            var data = await query
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    Timestamp = a.Timestamp,
                    Severity = a.Severity
                })
                .ToListAsync();

            return data.Cast<object>();
        }

        public async Task<IEnumerable<object>> GetPermissionsReportDataAsync(ReportFilterDto? filters = null)
        {
            var query = _context.UserClaims.AsQueryable();

            if (filters?.DateFrom.HasValue == true)
            {
                query = query.Where(uc => uc.CreatedAtUtc >= filters.DateFrom.Value);
            }

            if (filters?.DateTo.HasValue == true)
            {
                query = query.Where(uc => uc.CreatedAtUtc <= filters.DateTo.Value);
            }

            if (filters?.FieldFilters?.ContainsKey("ClaimType") == true)
            {
                if (filters.FieldFilters["ClaimType"] is string claimType)
                {
                    query = query.Where(uc => uc.ClaimType.Contains(claimType));
                }
            }

            if (filters?.FieldFilters?.ContainsKey("Category") == true)
            {
                if (filters.FieldFilters["Category"] is string category)
                {
                    query = query.Where(uc => uc.Category != null && uc.Category.Contains(category));
                }
            }

            if (filters?.MaxRecords.HasValue == true)
            {
                query = query.Take(filters.MaxRecords.Value);
            }

            var data = await query
                .Include(uc => uc.User)
                .Select(uc => new
                {
                    Id = uc.Id,
                    UserId = uc.UserId,
                    UserName = uc.User.UserName,
                    ClaimType = uc.ClaimType,
                    ClaimValue = uc.ClaimValue,
                    Description = uc.Description,
                    Category = uc.Category,
                    IsActive = uc.IsActive,
                    CreatedAtUtc = uc.CreatedAtUtc,
                    ExpiresAtUtc = uc.ExpiresAtUtc,
                    CreatedByUserId = uc.CreatedByUserId
                })
                .ToListAsync();

            return data.Cast<object>();
        }

        public async Task<int> CleanupOldReportExecutionsAsync(int retentionDays = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var executionsToDelete = await _context.ReportExecutions
                .Where(re => re.StartedAtUtc < cutoffDate)
                .ToListAsync();

            if (executionsToDelete.Any())
            {
                // Eliminar archivos físicos
                foreach (var execution in executionsToDelete)
                {
                    if (!string.IsNullOrEmpty(execution.FilePath) && File.Exists(execution.FilePath))
                    {
                        try
                        {
                            File.Delete(execution.FilePath);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with deletion
                        }
                    }
                }

                _context.ReportExecutions.RemoveRange(executionsToDelete);
                await _context.SaveChangesAsync();
            }

            return executionsToDelete.Count;
        }
    }
}
