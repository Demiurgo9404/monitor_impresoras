using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio principal de reportes
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly IReportDataService _reportDataService;
        private readonly IPdfReportGenerator _pdfGenerator;
        private readonly IExcelReportGenerator _excelGenerator;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReportService> _logger;
        private readonly string _reportsPath;

        public ReportService(
            ApplicationDbContext context,
            ITenantAccessor tenantAccessor,
            IReportDataService reportDataService,
            IPdfReportGenerator pdfGenerator,
            IExcelReportGenerator excelGenerator,
            IEmailService emailService,
            ILogger<ReportService> logger)
        {
            _context = context;
            _tenantAccessor = tenantAccessor;
            _reportDataService = reportDataService;
            _pdfGenerator = pdfGenerator;
            _excelGenerator = excelGenerator;
            _emailService = emailService;
            _logger = logger;
            
            // Crear directorio de reportes si no existe
            _reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            Directory.CreateDirectory(_reportsPath);
        }

        public async Task<ReportDto> GenerateReportAsync(GenerateReportDto generateDto)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new UnauthorizedAccessException("No tenant context available");
                }

                _logger.LogInformation("Generating report for project {ProjectId} in format {Format}", 
                    generateDto.ProjectId, generateDto.Format);

                // Verificar que el proyecto pertenece al tenant
                var project = await _context.Projects
                    .Include(p => p.Company)
                    .FirstOrDefaultAsync(p => p.Id == generateDto.ProjectId && p.TenantId == tenantId);

                if (project == null)
                {
                    throw new ArgumentException("Project not found or access denied");
                }

                // Obtener datos para el reporte
                var reportData = await _reportDataService.GetReportDataAsync(
                    generateDto.ProjectId, 
                    generateDto.PeriodStart, 
                    generateDto.PeriodEnd);

                // Crear entidad de reporte
                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ProjectId = generateDto.ProjectId,
                    Title = !string.IsNullOrEmpty(generateDto.Title) ? generateDto.Title : 
                           $"Reporte {generateDto.ReportType} - {project.Name}",
                    ReportType = generateDto.ReportType,
                    ReportPeriodStart = generateDto.PeriodStart,
                    ReportPeriodEnd = generateDto.PeriodEnd,
                    GeneratedAt = DateTime.UtcNow,
                    Status = ReportStatus.Generating,
                    FileFormat = generateDto.Format,
                    EmailRecipients = generateDto.EmailRecipients != null ? 
                                    string.Join(";", generateDto.EmailRecipients) : "",
                    TotalPrinters = reportData.Summary.TotalPrinters,
                    ActivePrinters = reportData.Summary.ActivePrinters,
                    TotalPrintsBW = reportData.Summary.TotalPagesBlackWhite,
                    TotalPrintsColor = reportData.Summary.TotalPagesColor,
                    TotalScans = reportData.Summary.TotalScans,
                    TotalCostBW = reportData.Costs.TotalCostBW,
                    TotalCostColor = reportData.Costs.TotalCostColor,
                    PrintersLowToner = reportData.Consumables.Count(c => c.Status == "Low" || c.Status == "Critical"),
                    PrintersNeedMaintenance = reportData.Printers.Count(p => p.NeedsMaintenance),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                try
                {
                    // Generar archivo(s) según el formato
                    byte[] fileData;
                    string fileName;
                    string contentType;

                    switch (generateDto.Format.ToUpper())
                    {
                        case "PDF":
                            fileData = await _pdfGenerator.GeneratePdfReportAsync(reportData);
                            fileName = $"reporte_{project.Name}_{generateDto.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                            contentType = "application/pdf";
                            break;

                        case "EXCEL":
                            fileData = await _excelGenerator.GenerateExcelReportAsync(reportData);
                            fileName = $"reporte_{project.Name}_{generateDto.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            break;

                        case "CSV":
                            fileData = await _excelGenerator.GenerateCsvReportAsync(reportData);
                            fileName = $"reporte_{project.Name}_{generateDto.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                            contentType = "text/csv";
                            break;

                        case "BOTH":
                            // Generar PDF y Excel
                            var pdfData = await _pdfGenerator.GeneratePdfReportAsync(reportData);
                            var excelData = await _excelGenerator.GenerateExcelReportAsync(reportData);
                            
                            // Guardar ambos archivos
                            var pdfFileName = $"reporte_{project.Name}_{generateDto.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                            var excelFileName = $"reporte_{project.Name}_{generateDto.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                            
                            await SaveReportFile(report.Id, pdfFileName, pdfData);
                            await SaveReportFile(report.Id, excelFileName, excelData);
                            
                            // Para "Both", devolvemos el PDF como principal
                            fileData = pdfData;
                            fileName = pdfFileName;
                            contentType = "application/pdf";
                            break;

                        default:
                            throw new ArgumentException($"Unsupported format: {generateDto.Format}");
                    }

                    // Guardar archivo
                    await SaveReportFile(report.Id, fileName, fileData);

                    // Actualizar reporte con información del archivo
                    report.FileName = fileName;
                    report.FilePath = GetReportFilePath(report.Id, fileName);
                    report.FileSizeBytes = fileData.Length;
                    report.Status = ReportStatus.Generated;
                    report.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Report generated successfully: {ReportId}", report.Id);

                    // Enviar por email si se solicita
                    if (generateDto.SendByEmail && generateDto.EmailRecipients?.Any() == true)
                    {
                        try
                        {
                            var emailSent = await _emailService.SendReportEmailAsync(
                                generateDto.EmailRecipients,
                                report.Title,
                                fileData,
                                fileName,
                                contentType,
                                $"Reporte generado para el proyecto {project.Name}"
                            );

                            if (emailSent)
                            {
                                report.EmailSent = true;
                                report.EmailSentAt = DateTime.UtcNow;
                                report.Status = ReportStatus.Sent;
                                await _context.SaveChangesAsync();
                                
                                _logger.LogInformation("Report email sent successfully for report {ReportId}", report.Id);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to send report email for report {ReportId}", report.Id);
                            }
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, "Error sending report email for report {ReportId}", report.Id);
                        }
                    }

                    return MapToReportDto(report);
                }
                catch (Exception ex)
                {
                    // Marcar reporte como fallido
                    report.Status = ReportStatus.Failed;
                    report.Notes = ex.Message;
                    report.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                throw;
            }
        }

        public async Task<ReportListDto> GetReportsAsync(int pageNumber = 1, int pageSize = 10, ReportFiltersDto? filters = null)
        {
            try
            {
                var query = _context.Reports
                    .Include(r => r.Project)
                    .ThenInclude(p => p.Company)
                    .AsQueryable();

                // Aplicar filtros
                if (filters != null)
                {
                    if (filters.ProjectId.HasValue)
                        query = query.Where(r => r.ProjectId == filters.ProjectId.Value);

                    if (!string.IsNullOrWhiteSpace(filters.ReportType))
                        query = query.Where(r => r.ReportType == filters.ReportType);

                    if (!string.IsNullOrWhiteSpace(filters.Status))
                        query = query.Where(r => r.Status == filters.Status);

                    if (!string.IsNullOrWhiteSpace(filters.Format))
                        query = query.Where(r => r.FileFormat == filters.Format);

                    if (filters.GeneratedFrom.HasValue)
                        query = query.Where(r => r.GeneratedAt >= filters.GeneratedFrom.Value);

                    if (filters.GeneratedTo.HasValue)
                        query = query.Where(r => r.GeneratedAt <= filters.GeneratedTo.Value);

                    if (filters.EmailSent.HasValue)
                        query = query.Where(r => r.EmailSent == filters.EmailSent.Value);

                    if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
                    {
                        query = query.Where(r => 
                            r.Title.Contains(filters.SearchTerm) ||
                            r.Project.Name.Contains(filters.SearchTerm) ||
                            r.Project.Company.Name.Contains(filters.SearchTerm));
                    }
                }

                var totalCount = await query.CountAsync();

                var reports = await query
                    .OrderByDescending(r => r.GeneratedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var reportDtos = reports.Select(MapToReportDto).ToList();

                return new ReportListDto
                {
                    Reports = reportDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports");
                throw;
            }
        }

        public async Task<ReportDto?> GetReportByIdAsync(Guid id)
        {
            try
            {
                var report = await _context.Reports
                    .Include(r => r.Project)
                    .ThenInclude(p => p.Company)
                    .FirstOrDefaultAsync(r => r.Id == id);

                return report != null ? MapToReportDto(report) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {ReportId}", id);
                throw;
            }
        }

        public async Task<(byte[] fileData, string fileName, string contentType)> DownloadReportAsync(Guid reportId)
        {
            try
            {
                var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == reportId);
                if (report == null)
                {
                    throw new ArgumentException("Report not found");
                }

                if (string.IsNullOrEmpty(report.FilePath) || !File.Exists(report.FilePath))
                {
                    throw new FileNotFoundException("Report file not found");
                }

                var fileData = await File.ReadAllBytesAsync(report.FilePath);
                var contentType = GetContentType(report.FileFormat);

                return (fileData, report.FileName, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading report {ReportId}", reportId);
                throw;
            }
        }

        public async Task<bool> DeleteReportAsync(Guid id)
        {
            try
            {
                var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
                if (report == null)
                {
                    return false;
                }

                // Eliminar archivo físico si existe
                if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
                {
                    File.Delete(report.FilePath);
                }

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Report deleted: {ReportId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
                throw;
            }
        }

        public async Task<bool> ResendReportByEmailAsync(Guid reportId, string[] emailRecipients)
        {
            try
            {
                var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == reportId);
                if (report == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(report.FilePath) || !File.Exists(report.FilePath))
                {
                    _logger.LogWarning("Report file not found for resending: {ReportId}", reportId);
                    return false;
                }

                // Leer archivo del reporte
                var fileData = await File.ReadAllBytesAsync(report.FilePath);
                var contentType = GetContentType(report.FileFormat);

                // Enviar por email
                var emailSent = await _emailService.SendReportEmailAsync(
                    emailRecipients,
                    report.Title,
                    fileData,
                    report.FileName,
                    contentType,
                    "Reenvío del reporte solicitado"
                );

                if (emailSent)
                {
                    // Actualizar información de envío
                    report.EmailRecipients = string.Join(";", emailRecipients);
                    report.EmailSent = true;
                    report.EmailSentAt = DateTime.UtcNow;
                    report.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Report resent successfully: {ReportId} to {Recipients}", 
                        reportId, string.Join(", ", emailRecipients));
                    
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to resend report email: {ReportId}", reportId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending report {ReportId}", reportId);
                throw;
            }
        }

        public async Task<List<ReportDto>> GetProjectReportsAsync(Guid projectId)
        {
            try
            {
                var reports = await _context.Reports
                    .Include(r => r.Project)
                    .ThenInclude(p => p.Company)
                    .Where(r => r.ProjectId == projectId)
                    .OrderByDescending(r => r.GeneratedAt)
                    .ToListAsync();

                return reports.Select(MapToReportDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project reports for {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<bool> HasAccessToReportAsync(Guid reportId, string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return false;
                }

                var report = await _context.Reports
                    .Include(r => r.Project)
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null)
                {
                    return false;
                }

                // SuperAdmin tiene acceso a todo
                if (user.Role == QopiqRoles.SuperAdmin)
                {
                    return true;
                }

                // CompanyAdmin tiene acceso a reportes de su empresa
                if (user.Role == QopiqRoles.CompanyAdmin && user.CompanyId == report.Project.CompanyId)
                {
                    return true;
                }

                // Verificar asignación al proyecto
                var projectUser = await _context.ProjectUsers
                    .FirstOrDefaultAsync(pu => pu.ProjectId == report.ProjectId && pu.UserId == userId && pu.IsActive);

                return projectUser != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking report access for user {UserId} and report {ReportId}", userId, reportId);
                return false;
            }
        }

        private async Task SaveReportFile(Guid reportId, string fileName, byte[] fileData)
        {
            var reportDirectory = Path.Combine(_reportsPath, reportId.ToString());
            Directory.CreateDirectory(reportDirectory);
            
            var filePath = Path.Combine(reportDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, fileData);
        }

        private string GetReportFilePath(Guid reportId, string fileName)
        {
            return Path.Combine(_reportsPath, reportId.ToString(), fileName);
        }

        private string GetContentType(string format)
        {
            return format.ToUpper() switch
            {
                "PDF" => "application/pdf",
                "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "CSV" => "text/csv",
                _ => "application/octet-stream"
            };
        }

        private ReportDto MapToReportDto(Report report)
        {
            return new ReportDto
            {
                Id = report.Id,
                ProjectId = report.ProjectId,
                ProjectName = report.Project?.Name ?? "",
                CompanyName = report.Project?.Company?.Name ?? "",
                Title = report.Title,
                ReportType = report.ReportType,
                PeriodStart = report.ReportPeriodStart,
                PeriodEnd = report.ReportPeriodEnd,
                GeneratedAt = report.GeneratedAt,
                Status = report.Status,
                FileFormat = report.FileFormat,
                FileName = report.FileName,
                FileSizeBytes = report.FileSizeBytes,
                EmailSent = report.EmailSent,
                EmailSentAt = report.EmailSentAt,
                EmailRecipients = report.EmailRecipients,
                Notes = report.Notes,
                TotalPrinters = report.TotalPrinters,
                ActivePrinters = report.ActivePrinters,
                TotalPrintsBW = report.TotalPrintsBW,
                TotalPrintsColor = report.TotalPrintsColor,
                TotalScans = report.TotalScans,
                TotalCostBW = report.TotalCostBW,
                TotalCostColor = report.TotalCostColor,
                PrintersLowToner = report.PrintersLowToner,
                PrintersNeedMaintenance = report.PrintersNeedMaintenance
            };
        }
    }
}
