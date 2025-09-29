using Cronos;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using System.Text.Json;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para gestión de reportes programados
    /// </summary>
    public class ScheduledReportService : IScheduledReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ScheduledReportService> _logger;

        public ScheduledReportService(
            ApplicationDbContext context,
            IReportService reportService,
            IEmailService emailService,
            ILogger<ScheduledReportService> logger)
        {
            _context = context;
            _reportService = reportService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<ScheduledReportDto>> GetUserScheduledReportsAsync(string userId)
        {
            _logger.LogInformation("Obteniendo reportes programados para usuario: {UserId}", userId);

            var scheduledReports = await _context.ScheduledReports
                .Include(sr => sr.ReportTemplate)
                .Where(sr => sr.CreatedByUserId == userId && sr.IsActive)
                .OrderBy(sr => sr.NextExecutionUtc)
                .Select(sr => new ScheduledReportDto
                {
                    Id = sr.Id,
                    Name = sr.Name,
                    Description = sr.Description,
                    ReportTemplateId = sr.ReportTemplateId,
                    ReportTemplateName = sr.ReportTemplate.Name,
                    CronExpression = sr.CronExpression,
                    Format = sr.Format,
                    Recipients = sr.Recipients,
                    IsActive = sr.IsActive,
                    LastSuccessfulExecutionUtc = sr.LastSuccessfulExecutionUtc,
                    NextExecutionUtc = sr.NextExecutionUtc,
                    CreatedAtUtc = sr.CreatedAtUtc
                })
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} reportes programados para el usuario", scheduledReports.Count);
            return scheduledReports;
        }

        public async Task<ScheduledReportDto?> GetScheduledReportByIdAsync(int scheduledReportId, string userId)
        {
            _logger.LogInformation("Obteniendo reporte programado {ScheduledReportId} para usuario {UserId}",
                scheduledReportId, userId);

            var scheduledReport = await _context.ScheduledReports
                .Include(sr => sr.ReportTemplate)
                .FirstOrDefaultAsync(sr => sr.Id == scheduledReportId && sr.CreatedByUserId == userId);

            if (scheduledReport == null)
            {
                return null;
            }

            return new ScheduledReportDto
            {
                Id = scheduledReport.Id,
                Name = scheduledReport.Name,
                Description = scheduledReport.Description,
                ReportTemplateId = scheduledReport.ReportTemplateId,
                ReportTemplateName = scheduledReport.ReportTemplate.Name,
                CronExpression = scheduledReport.CronExpression,
                Format = scheduledReport.Format,
                Recipients = scheduledReport.Recipients,
                IsActive = scheduledReport.IsActive,
                LastSuccessfulExecutionUtc = scheduledReport.LastSuccessfulExecutionUtc,
                NextExecutionUtc = scheduledReport.NextExecutionUtc,
                CreatedAtUtc = scheduledReport.CreatedAtUtc
            };
        }

        public async Task<ScheduledReportDto> CreateScheduledReportAsync(CreateScheduledReportDto request, string userId)
        {
            _logger.LogInformation("Creando reporte programado para usuario: {UserId}", userId);

            // Validar template
            var template = await _context.ReportTemplates.FindAsync(request.ReportTemplateId);
            if (template == null || !template.IsActive)
            {
                throw new ArgumentException("Template de reporte no encontrado o inactivo");
            }

            // Validar CRON expression
            if (!CronExpression.TryParse(request.CronExpression, out _))
            {
                throw new ArgumentException("Expresión CRON inválida");
            }

            // Crear reporte programado
            var scheduledReport = new ScheduledReport
            {
                ReportTemplateId = request.ReportTemplateId,
                CreatedByUserId = userId,
                Name = request.Name,
                Description = request.Description,
                CronExpression = request.CronExpression,
                Format = request.Format,
                Recipients = request.Recipients,
                FixedParameters = JsonSerializer.Serialize(request.FixedParameters ?? new Dictionary<string, object>()),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                NextExecutionUtc = CalculateNextExecution(request.CronExpression)
            };

            _context.ScheduledReports.Add(scheduledReport);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reporte programado creado: {ScheduledReportId}", scheduledReport.Id);

            return new ScheduledReportDto
            {
                Id = scheduledReport.Id,
                Name = scheduledReport.Name,
                Description = scheduledReport.Description,
                ReportTemplateId = scheduledReport.ReportTemplateId,
                ReportTemplateName = template.Name,
                CronExpression = scheduledReport.CronExpression,
                Format = scheduledReport.Format,
                Recipients = scheduledReport.Recipients,
                IsActive = scheduledReport.IsActive,
                LastSuccessfulExecutionUtc = scheduledReport.LastSuccessfulExecutionUtc,
                NextExecutionUtc = scheduledReport.NextExecutionUtc,
                CreatedAtUtc = scheduledReport.CreatedAtUtc
            };
        }

        public async Task<bool> UpdateScheduledReportAsync(int scheduledReportId, UpdateScheduledReportDto request, string userId)
        {
            _logger.LogInformation("Actualizando reporte programado {ScheduledReportId} para usuario {UserId}",
                scheduledReportId, userId);

            var scheduledReport = await _context.ScheduledReports
                .FirstOrDefaultAsync(sr => sr.Id == scheduledReportId && sr.CreatedByUserId == userId);

            if (scheduledReport == null)
            {
                return false;
            }

            // Actualizar campos
            if (!string.IsNullOrEmpty(request.Name))
                scheduledReport.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Description))
                scheduledReport.Description = request.Description;

            if (!string.IsNullOrEmpty(request.CronExpression))
            {
                if (!CronExpression.TryParse(request.CronExpression, out _))
                {
                    throw new ArgumentException("Expresión CRON inválida");
                }
                scheduledReport.CronExpression = request.CronExpression;
                scheduledReport.NextExecutionUtc = CalculateNextExecution(request.CronExpression);
            }

            if (!string.IsNullOrEmpty(request.Format))
                scheduledReport.Format = request.Format;

            if (!string.IsNullOrEmpty(request.Recipients))
                scheduledReport.Recipients = request.Recipients;

            if (request.IsActive.HasValue)
                scheduledReport.IsActive = request.IsActive.Value;

            if (request.FixedParameters != null)
                scheduledReport.FixedParameters = JsonSerializer.Serialize(request.FixedParameters);

            scheduledReport.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reporte programado {ScheduledReportId} actualizado exitosamente", scheduledReportId);
            return true;
        }

        public async Task<bool> DeleteScheduledReportAsync(int scheduledReportId, string userId)
        {
            _logger.LogInformation("Eliminando reporte programado {ScheduledReportId} para usuario {UserId}",
                scheduledReportId, userId);

            var scheduledReport = await _context.ScheduledReports
                .FirstOrDefaultAsync(sr => sr.Id == scheduledReportId && sr.CreatedByUserId == userId);

            if (scheduledReport == null)
            {
                return false;
            }

            scheduledReport.IsActive = false;
            scheduledReport.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reporte programado {ScheduledReportId} eliminado exitosamente", scheduledReportId);
            return true;
        }

        public async Task<IEnumerable<ScheduledReport>> GetDueScheduledReportsAsync()
        {
            var now = DateTime.UtcNow;

            var dueReports = await _context.ScheduledReports
                .Include(sr => sr.ReportTemplate)
                .Where(sr => sr.IsActive &&
                           sr.NextExecutionUtc.HasValue &&
                           sr.NextExecutionUtc.Value <= now)
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} reportes programados vencidos", dueReports.Count);
            return dueReports;
        }

        public async Task ExecuteScheduledReportAsync(ScheduledReport scheduledReport, string executedBy = "system")
        {
            _logger.LogInformation("Ejecutando reporte programado: {ScheduledReportId}", scheduledReport.Id);

            try
            {
                // Crear request para el reporte
                var request = new ReportRequestDto
                {
                    ReportTemplateId = scheduledReport.ReportTemplateId,
                    Format = scheduledReport.Format,
                    Parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(scheduledReport.FixedParameters ?? "{}")
                };

                // Ejecutar reporte
                var result = await _reportService.GenerateReportAsync(request, scheduledReport.CreatedByUserId, "127.0.0.1");

                // Actualizar estado
                scheduledReport.LastSuccessfulExecutionUtc = DateTime.UtcNow;
                scheduledReport.NextExecutionUtc = CalculateNextExecution(scheduledReport.CronExpression);

                // Si hay destinatarios, enviar por email
                if (!string.IsNullOrEmpty(scheduledReport.Recipients))
                {
                    await SendScheduledReportByEmailAsync(scheduledReport, result);
                }

                _logger.LogInformation("Reporte programado {ScheduledReportId} ejecutado exitosamente", scheduledReport.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar reporte programado: {ScheduledReportId}", scheduledReport.Id);

                // Crear ejecución fallida para auditoría
                var execution = new ReportExecution
                {
                    ReportTemplateId = scheduledReport.ReportTemplateId,
                    ExecutedByUserId = scheduledReport.CreatedByUserId,
                    Format = scheduledReport.Format,
                    Status = "failed",
                    ErrorMessage = ex.Message,
                    StartedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow,
                    ExecutedByIp = "127.0.0.1"
                };

                _context.ReportExecutions.Add(execution);
            }

            await _context.SaveChangesAsync();
        }

        public DateTime CalculateNextExecution(string cronExpression)
        {
            try
            {
                var cron = CronExpression.Parse(cronExpression);
                return cron.GetNextOccurrence(DateTime.UtcNow) ?? DateTime.UtcNow.AddHours(1);
            }
            catch
            {
                // Si la expresión CRON es inválida, usar cada hora
                return DateTime.UtcNow.AddHours(1);
            }
        }

        public async Task SendScheduledReportByEmailAsync(ScheduledReport scheduledReport, ReportResultDto result)
        {
            try
            {
                if (string.IsNullOrEmpty(scheduledReport.Recipients))
                {
                    return;
                }

                var recipients = scheduledReport.Recipients.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToList();

                if (!recipients.Any())
                {
                    return;
                }

                // Descargar el archivo del reporte
                var (fileContent, contentType, fileName) = await _reportService.DownloadReportAsync(result.ExecutionId, scheduledReport.CreatedByUserId);

                // Enviar email
                await _emailService.SendReportEmailAsync(
                    recipients,
                    $"Reporte Programado: {scheduledReport.Name}",
                    $"Se ha generado automáticamente el reporte '{scheduledReport.Name}' en formato {scheduledReport.Format.ToUpper()}.",
                    fileContent,
                    fileName,
                    contentType);

                _logger.LogInformation("Reporte programado {ScheduledReportId} enviado por email a {RecipientCount} destinatarios",
                    scheduledReport.Id, recipients.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar reporte programado {ScheduledReportId} por email", scheduledReport.Id);
            }
        }
    }
}
