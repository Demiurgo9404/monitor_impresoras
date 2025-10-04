using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de reportes programados
    /// </summary>
    public class ScheduledReportService : IScheduledReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly IReportService _reportService;
        private readonly ILogger<ScheduledReportService> _logger;

        public ScheduledReportService(
            ApplicationDbContext context,
            ITenantAccessor tenantAccessor,
            IReportService reportService,
            ILogger<ScheduledReportService> logger)
        {
            _context = context;
            _tenantAccessor = tenantAccessor;
            _reportService = reportService;
            _logger = logger;
        }

        public async Task<ScheduledReportDto> CreateScheduledReportAsync(CreateScheduledReportDto createDto)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new UnauthorizedAccessException("No tenant context available");
                }

                // Verificar que el proyecto pertenece al tenant
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == createDto.ProjectId && p.TenantId == tenantId);

                if (project == null)
                {
                    throw new ArgumentException("Project not found or access denied");
                }

                var scheduledReport = new ScheduledReport
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ProjectId = createDto.ProjectId,
                    Name = createDto.Name,
                    ReportType = createDto.ReportType,
                    Schedule = createDto.Schedule,
                    FileFormat = createDto.Format,
                    EmailRecipients = string.Join(";", createDto.EmailRecipients),
                    IncludeCounters = createDto.IncludeCounters,
                    IncludeConsumables = createDto.IncludeConsumables,
                    IncludeCosts = createDto.IncludeCosts,
                    IncludeCharts = createDto.IncludeCharts,
                    IsActive = true,
                    NextRun = CalculateNextRun(createDto.Schedule),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ScheduledReports.Add(scheduledReport);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Scheduled report created: {ScheduledReportId} for project {ProjectId}", 
                    scheduledReport.Id, createDto.ProjectId);

                return MapToScheduledReportDto(scheduledReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scheduled report");
                throw;
            }
        }

        public async Task<List<ScheduledReportDto>> GetScheduledReportsAsync()
        {
            try
            {
                var scheduledReports = await _context.ScheduledReports
                    .Include(sr => sr.Project)
                    .OrderBy(sr => sr.Name)
                    .ToListAsync();

                return scheduledReports.Select(MapToScheduledReportDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scheduled reports");
                throw;
            }
        }

        public async Task<ScheduledReportDto?> GetScheduledReportByIdAsync(Guid id)
        {
            try
            {
                var scheduledReport = await _context.ScheduledReports
                    .Include(sr => sr.Project)
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                return scheduledReport != null ? MapToScheduledReportDto(scheduledReport) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scheduled report {ScheduledReportId}", id);
                throw;
            }
        }

        public async Task<ScheduledReportDto?> UpdateScheduledReportAsync(Guid id, CreateScheduledReportDto updateDto)
        {
            try
            {
                var scheduledReport = await _context.ScheduledReports
                    .Include(sr => sr.Project)
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (scheduledReport == null)
                {
                    return null;
                }

                scheduledReport.Name = updateDto.Name;
                scheduledReport.ReportType = updateDto.ReportType;
                scheduledReport.Schedule = updateDto.Schedule;
                scheduledReport.FileFormat = updateDto.Format;
                scheduledReport.EmailRecipients = string.Join(";", updateDto.EmailRecipients);
                scheduledReport.IncludeCounters = updateDto.IncludeCounters;
                scheduledReport.IncludeConsumables = updateDto.IncludeConsumables;
                scheduledReport.IncludeCosts = updateDto.IncludeCosts;
                scheduledReport.IncludeCharts = updateDto.IncludeCharts;
                scheduledReport.NextRun = CalculateNextRun(updateDto.Schedule);
                scheduledReport.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Scheduled report updated: {ScheduledReportId}", id);

                return MapToScheduledReportDto(scheduledReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scheduled report {ScheduledReportId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteScheduledReportAsync(Guid id)
        {
            try
            {
                var scheduledReport = await _context.ScheduledReports.FirstOrDefaultAsync(sr => sr.Id == id);
                if (scheduledReport == null)
                {
                    return false;
                }

                _context.ScheduledReports.Remove(scheduledReport);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Scheduled report deleted: {ScheduledReportId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting scheduled report {ScheduledReportId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleScheduledReportAsync(Guid id, bool isActive)
        {
            try
            {
                var scheduledReport = await _context.ScheduledReports.FirstOrDefaultAsync(sr => sr.Id == id);
                if (scheduledReport == null)
                {
                    return false;
                }

                scheduledReport.IsActive = isActive;
                scheduledReport.UpdatedAt = DateTime.UtcNow;

                if (isActive)
                {
                    scheduledReport.NextRun = CalculateNextRun(scheduledReport.Schedule);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Scheduled report {Action}: {ScheduledReportId}", 
                    isActive ? "activated" : "deactivated", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling scheduled report {ScheduledReportId}", id);
                throw;
            }
        }

        public async Task<List<ScheduledReportDto>> GetReportsToExecuteAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var reportsToExecute = await _context.ScheduledReports
                    .Include(sr => sr.Project)
                    .Where(sr => sr.IsActive && sr.NextRun <= now)
                    .ToListAsync();

                return reportsToExecute.Select(MapToScheduledReportDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports to execute");
                throw;
            }
        }

        public async Task<bool> ExecuteScheduledReportAsync(Guid scheduledReportId)
        {
            try
            {
                var scheduledReport = await _context.ScheduledReports
                    .Include(sr => sr.Project)
                    .FirstOrDefaultAsync(sr => sr.Id == scheduledReportId);

                if (scheduledReport == null || !scheduledReport.IsActive)
                {
                    return false;
                }

                _logger.LogInformation("Executing scheduled report: {ScheduledReportId}", scheduledReportId);

                // Calcular período del reporte basado en el tipo
                var (periodStart, periodEnd) = CalculateReportPeriod(scheduledReport.ReportType);

                // Generar reporte
                var generateDto = new GenerateReportDto
                {
                    ProjectId = scheduledReport.ProjectId,
                    ReportType = scheduledReport.ReportType,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Format = scheduledReport.FileFormat,
                    Title = $"{scheduledReport.Name} - {DateTime.UtcNow:yyyy-MM-dd}",
                    IncludeCounters = scheduledReport.IncludeCounters,
                    IncludeConsumables = scheduledReport.IncludeConsumables,
                    IncludeCosts = scheduledReport.IncludeCosts,
                    IncludeCharts = scheduledReport.IncludeCharts,
                    SendByEmail = !string.IsNullOrEmpty(scheduledReport.EmailRecipients),
                    EmailRecipients = !string.IsNullOrEmpty(scheduledReport.EmailRecipients) ? 
                                    scheduledReport.EmailRecipients.Split(';') : null
                };

                // Establecer contexto del tenant temporalmente
                var originalTenant = _tenantAccessor.TenantId;
                ((TenantAccessor)_tenantAccessor).SetTenant(scheduledReport.TenantId);

                try
                {
                    var report = await _reportService.GenerateReportAsync(generateDto);
                    
                    // Actualizar información de ejecución
                    var nextRun = CalculateNextRun(scheduledReport.Schedule);
                    await UpdateLastRunAsync(scheduledReportId, DateTime.UtcNow, nextRun);

                    _logger.LogInformation("Scheduled report executed successfully: {ScheduledReportId}, generated report: {ReportId}", 
                        scheduledReportId, report.Id);

                    return true;
                }
                finally
                {
                    // Restaurar contexto del tenant original
                    if (!string.IsNullOrEmpty(originalTenant))
                    {
                        ((TenantAccessor)_tenantAccessor).SetTenant(originalTenant);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scheduled report {ScheduledReportId}", scheduledReportId);
                
                // Marcar como fallido y programar siguiente ejecución
                try
                {
                    var scheduledReport = await _context.ScheduledReports.FirstOrDefaultAsync(sr => sr.Id == scheduledReportId);
                    if (scheduledReport != null)
                    {
                        var nextRun = CalculateNextRun(scheduledReport.Schedule);
                        await UpdateLastRunAsync(scheduledReportId, DateTime.UtcNow, nextRun);
                    }
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Error updating failed scheduled report {ScheduledReportId}", scheduledReportId);
                }

                return false;
            }
        }

        public async Task UpdateLastRunAsync(Guid scheduledReportId, DateTime lastRun, DateTime? nextRun)
        {
            try
            {
                var scheduledReport = await _context.ScheduledReports.FirstOrDefaultAsync(sr => sr.Id == scheduledReportId);
                if (scheduledReport != null)
                {
                    scheduledReport.LastRun = lastRun;
                    scheduledReport.NextRun = nextRun;
                    scheduledReport.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last run for scheduled report {ScheduledReportId}", scheduledReportId);
                throw;
            }
        }

        private DateTime? CalculateNextRun(string schedule)
        {
            try
            {
                // Implementación simplificada de cron
                // En producción usar NCrontab o similar
                var now = DateTime.UtcNow;

                return schedule switch
                {
                    "0 0 * * *" => now.Date.AddDays(1), // Diario a medianoche
                    "0 0 * * 1" => GetNextWeekday(now, DayOfWeek.Monday), // Lunes
                    "0 0 1 * *" => new DateTime(now.Year, now.Month, 1).AddMonths(1), // Primer día del mes
                    "0 0 1 1 *" => new DateTime(now.Year + 1, 1, 1), // Primer día del año
                    _ => now.AddHours(1) // Default: cada hora
                };
            }
            catch
            {
                return DateTime.UtcNow.AddHours(1); // Fallback
            }
        }

        private DateTime GetNextWeekday(DateTime date, DayOfWeek targetDay)
        {
            var daysUntilTarget = ((int)targetDay - (int)date.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0) daysUntilTarget = 7; // Si es el mismo día, programar para la próxima semana
            return date.Date.AddDays(daysUntilTarget);
        }

        private (DateTime periodStart, DateTime periodEnd) CalculateReportPeriod(string reportType)
        {
            var now = DateTime.UtcNow;

            return reportType switch
            {
                "Daily" => (now.Date.AddDays(-1), now.Date.AddSeconds(-1)),
                "Weekly" => (now.Date.AddDays(-7), now.Date.AddSeconds(-1)),
                "Monthly" => (new DateTime(now.Year, now.Month, 1).AddMonths(-1), 
                            new DateTime(now.Year, now.Month, 1).AddSeconds(-1)),
                "Quarterly" => (new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1).AddMonths(-3),
                              new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1).AddSeconds(-1)),
                "Yearly" => (new DateTime(now.Year - 1, 1, 1), new DateTime(now.Year, 1, 1).AddSeconds(-1)),
                _ => (now.Date.AddMonths(-1), now.Date.AddSeconds(-1))
            };
        }

        private ScheduledReportDto MapToScheduledReportDto(ScheduledReport scheduledReport)
        {
            return new ScheduledReportDto
            {
                Id = scheduledReport.Id,
                ProjectId = scheduledReport.ProjectId,
                ProjectName = scheduledReport.Project?.Name ?? "",
                Name = scheduledReport.Name,
                ReportType = scheduledReport.ReportType,
                Schedule = scheduledReport.Schedule,
                Format = scheduledReport.FileFormat,
                IsActive = scheduledReport.IsActive,
                LastRun = scheduledReport.LastRun,
                NextRun = scheduledReport.NextRun,
                EmailRecipients = scheduledReport.EmailRecipients,
                IncludeCounters = scheduledReport.IncludeCounters,
                IncludeConsumables = scheduledReport.IncludeConsumables,
                IncludeCosts = scheduledReport.IncludeCosts,
                IncludeCharts = scheduledReport.IncludeCharts,
                CreatedAt = scheduledReport.CreatedAt,
                UpdatedAt = scheduledReport.UpdatedAt
            };
        }
    }
}
