using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using System.Text.Json;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para gestión de reportes
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportRepository _reportRepository;
        private readonly IPermissionService _permissionService;
        private readonly PdfExportService _pdfExportService;
        private readonly ExcelExportService _excelExportService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            ApplicationDbContext context,
            IReportRepository reportRepository,
            IPermissionService permissionService,
            PdfExportService pdfExportService,
            ExcelExportService excelExportService,
            ILogger<ReportService> logger)
        {
            _context = context;
            _reportRepository = reportRepository;
            _permissionService = permissionService;
            _pdfExportService = pdfExportService;
            _excelExportService = excelExportService;
            _logger = logger;
        }

        public async Task<IEnumerable<ReportTemplateDto>> GetAvailableReportsAsync(string userId)
        {
            _logger.LogInformation("Obteniendo reportes disponibles para usuario: {UserId}", userId);

            var templates = await _reportRepository.GetAvailableReportTemplatesAsync(userId);

            var templateDtos = templates.Select(t => new ReportTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                EntityType = t.EntityType,
                SupportedFormats = t.SupportedFormats,
                RequiredClaim = t.RequiredClaim,
                IsActive = t.IsActive,
                EstimatedExecutionTimeSeconds = t.EstimatedExecutionTimeSeconds,
                CreatedAtUtc = t.CreatedAtUtc,
                UpdatedAtUtc = t.UpdatedAtUtc,
                CreatedByUserName = t.CreatedByUser?.UserName
            });

            _logger.LogInformation("Se encontraron {Count} reportes disponibles para el usuario", templateDtos.Count());
            return templateDtos;
        }

        public async Task<ReportResultDto> GenerateReportAsync(ReportRequestDto request, string userId, string ipAddress)
        {
            _logger.LogInformation("Generando reporte {ReportTemplateId} para usuario {UserId}", request.ReportTemplateId, userId);

            // Verificar permisos
            var template = await _reportRepository.GetReportTemplateByIdAsync(request.ReportTemplateId);
            if (template == null || !template.IsActive)
            {
                throw new ArgumentException("Template de reporte no encontrado o inactivo");
            }

            if (!string.IsNullOrEmpty(template.RequiredClaim))
            {
                var hasClaim = await _permissionService.UserHasClaimAsync(userId, template.RequiredClaim);
                if (!hasClaim)
                {
                    throw new UnauthorizedAccessException($"Se requiere el claim: {template.RequiredClaim}");
                }
            }

            // Crear ejecución
            var execution = new ReportExecution
            {
                ReportTemplateId = request.ReportTemplateId,
                ExecutedByUserId = userId,
                Parameters = JsonSerializer.Serialize(request.Parameters ?? new Dictionary<string, object>()),
                Format = request.Format,
                Status = "pending",
                StartedAtUtc = DateTime.UtcNow,
                ExecutedByIp = ipAddress
            };

            execution = await _reportRepository.CreateReportExecutionAsync(execution);

            // Ejecutar generación en background
            _ = Task.Run(async () =>
            {
                await GenerateReportInBackgroundAsync(execution, request);
            });

            _logger.LogInformation("Ejecución de reporte {ExecutionId} iniciada", execution.Id);

            return new ReportResultDto
            {
                ExecutionId = execution.Id,
                ReportTemplateId = execution.ReportTemplateId,
                ReportName = template.Name,
                Format = execution.Format,
                Status = execution.Status,
                RecordCount = execution.RecordCount,
                FileSize = execution.FileSize,
                StartedAt = execution.StartedAtUtc,
                CompletedAt = execution.CompletedAtUtc,
                ExecutionTimeSeconds = execution.ExecutionTimeSeconds,
                ErrorMessage = execution.ErrorMessage
            };
        }

        public async Task<IEnumerable<ReportHistoryDto>> GetReportHistoryAsync(string userId, int page = 1, int pageSize = 20)
        {
            _logger.LogInformation("Obteniendo historial de reportes para usuario: {UserId}", userId);

            var executions = await _reportRepository.GetUserReportExecutionsAsync(userId, page, pageSize);

            var historyDtos = executions.Select(e => new ReportHistoryDto
            {
                ExecutionId = e.Id,
                ReportName = e.ReportTemplate.Name,
                Format = e.Format,
                Status = e.Status,
                RecordCount = e.RecordCount,
                StartedAt = e.StartedAtUtc,
                CompletedAt = e.CompletedAtUtc,
                ExecutionTimeSeconds = e.ExecutionTimeSeconds,
                DownloadUrl = e.DownloadUrl,
                ErrorMessage = e.ErrorMessage
            });

            _logger.LogInformation("Se encontraron {Count} ejecuciones en el historial", historyDtos.Count());
            return historyDtos;
        }

        public async Task<ReportResultDto?> GetReportExecutionAsync(int executionId, string userId)
        {
            _logger.LogInformation("Obteniendo detalles de ejecución {ExecutionId} para usuario {UserId}", executionId, userId);

            var execution = await _reportRepository.GetReportExecutionByIdAsync(executionId);
            if (execution == null || execution.ExecutedByUserId != userId)
            {
                return null;
            }

            return new ReportResultDto
            {
                ExecutionId = execution.Id,
                ReportTemplateId = execution.ReportTemplateId,
                ReportName = execution.ReportTemplate.Name,
                Format = execution.Format,
                Status = execution.Status,
                RecordCount = execution.RecordCount,
                FileSize = execution.FileSize,
                DownloadUrl = execution.DownloadUrl,
                StartedAt = execution.StartedAtUtc,
                CompletedAt = execution.CompletedAtUtc,
                ExecutionTimeSeconds = execution.ExecutionTimeSeconds,
                ErrorMessage = execution.ErrorMessage
            };
        }

        public async Task<(byte[] Content, string ContentType, string FileName)> DownloadReportAsync(int executionId, string userId)
        {
            _logger.LogInformation("Descargando reporte {ExecutionId} para usuario {UserId}", executionId, userId);

            var execution = await _reportRepository.GetReportExecutionByIdAsync(executionId);
            if (execution == null || execution.ExecutedByUserId != userId)
            {
                throw new FileNotFoundException("Reporte no encontrado");
            }

            if (execution.Status != "completed" || string.IsNullOrEmpty(execution.FilePath))
            {
                throw new InvalidOperationException("Reporte no disponible para descarga");
            }

            // Leer archivo
            var fileContent = await File.ReadAllBytesAsync(execution.FilePath);

            var contentType = GetContentType(execution.Format);
            var fileName = $"{execution.ReportTemplate.Name}_{execution.StartedAtUtc:yyyyMMdd_HHmmss}.{execution.Format}";

            _logger.LogInformation("Reporte {ExecutionId} descargado exitosamente", executionId);
            return (fileContent, contentType, fileName);
        }

        public async Task<ReportStatisticsDto> GetReportStatisticsAsync(string userId)
        {
            _logger.LogInformation("Obteniendo estadísticas de reportes para usuario: {UserId}", userId);

            return await _reportRepository.GetReportStatisticsAsync(userId);
        }

        public async Task<bool> CancelReportExecutionAsync(int executionId, string userId)
        {
            _logger.LogInformation("Cancelando ejecución {ExecutionId} para usuario {UserId}", executionId, userId);

            var execution = await _reportRepository.GetReportExecutionByIdAsync(executionId);
            if (execution == null || execution.ExecutedByUserId != userId)
            {
                return false;
            }

            if (execution.Status != "running" && execution.Status != "pending")
            {
                return false;
            }

            execution.Status = "cancelled";
            execution.ErrorMessage = "Cancelado por el usuario";
            await _reportRepository.UpdateReportExecutionAsync(execution);

            _logger.LogInformation("Ejecución {ExecutionId} cancelada exitosamente", executionId);
            return true;
        }

        /// <summary>
        /// Genera el reporte en segundo plano
        /// </summary>
        private async Task GenerateReportInBackgroundAsync(ReportExecution execution, ReportRequestDto request)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                execution.Status = "running";
                await _reportRepository.UpdateReportExecutionAsync(execution);

                // Obtener datos según el tipo de reporte
                var data = await GetReportDataAsync(execution.ReportTemplate, request);

                // Generar archivo según formato
                var (fileContent, filePath) = await GenerateReportFileAsync(execution, data, request.Format);

                execution.Status = "completed";
                execution.RecordCount = data.Count();
                execution.FileSize = fileContent.Length;
                execution.FilePath = filePath;
                execution.DownloadUrl = $"/api/v1/reports/{execution.Id}/download";
                execution.CompletedAtUtc = DateTime.UtcNow;
                execution.ExecutionTimeSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

                _logger.LogInformation("Reporte {ExecutionId} generado exitosamente con {RecordCount} registros",
                    execution.Id, execution.RecordCount);
            }
            catch (Exception ex)
            {
                execution.Status = "failed";
                execution.ErrorMessage = ex.Message;
                execution.CompletedAtUtc = DateTime.UtcNow;
                execution.ExecutionTimeSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

                _logger.LogError(ex, "Error al generar reporte {ExecutionId}", execution.Id);
            }

            await _reportRepository.UpdateReportExecutionAsync(execution);
        }

        /// <summary>
        /// Obtiene los datos del reporte según el template
        /// </summary>
        private async Task<IEnumerable<object>> GetReportDataAsync(ReportTemplate template, ReportRequestDto request)
        {
            return template.EntityType switch
            {
                "Printer" => await _reportRepository.GetPrinterReportDataAsync(request.Filters),
                "User" => await _reportRepository.GetUserReportDataAsync(request.Filters),
                "AuditLog" => await _reportRepository.GetAuditReportDataAsync(request.Filters),
                "UserClaim" => await _reportRepository.GetPermissionsReportDataAsync(request.Filters),
                _ => throw new NotSupportedException($"Tipo de entidad no soportado: {template.EntityType}")
            };
        }

        /// <summary>
        /// Genera el archivo del reporte en el formato especificado
        /// </summary>
        private async Task<(byte[] Content, string FilePath)> GenerateReportFileAsync(
            ReportExecution execution, IEnumerable<object> data, string format)
        {
            var tempFileName = $"{execution.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
            var tempPath = Path.Combine(Path.GetTempPath(), "reports", tempFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

            byte[] content;
            string contentType;

            switch (format.ToLower())
            {
                case "json":
                    content = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                    contentType = "application/json";
                    break;

                case "csv":
                    content = GenerateCsvContent(data);
                    contentType = "text/csv";
                    break;

                case "pdf":
                    var user = await _context.Users.FindAsync(execution.ExecutedByUserId);
                    var userName = user?.UserName ?? "Sistema";
                    var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(execution.Parameters ?? "{}");

                    content = execution.ReportTemplate.EntityType switch
                    {
                        "Printer" => await _pdfExportService.GeneratePrinterPdfAsync(data, userName, parameters),
                        "User" => await _pdfExportService.GenerateUserPdfAsync(data, userName, parameters),
                        "AuditLog" => await _pdfExportService.GenerateAuditPdfAsync(data, userName, parameters),
                        "UserClaim" => await _pdfExportService.GeneratePermissionsPdfAsync(data, userName, parameters),
                        _ => await _pdfExportService.GeneratePdfAsync(
                            execution.ReportTemplate.Name,
                            execution.ReportTemplate.Description ?? "Reporte generado automáticamente",
                            data, userName, parameters)
                    };
                    contentType = "application/pdf";
                    break;

                case "excel":
                case "xlsx":
                    var userExcel = await _context.Users.FindAsync(execution.ExecutedByUserId);
                    var userNameExcel = userExcel?.UserName ?? "Sistema";
                    var parametersExcel = JsonSerializer.Deserialize<Dictionary<string, object>>(execution.Parameters ?? "{}");

                    content = execution.ReportTemplate.EntityType switch
                    {
                        "Printer" => await _excelExportService.GeneratePrinterExcelAsync(data, userNameExcel, parametersExcel),
                        "User" => await _excelExportService.GenerateUserExcelAsync(data, userNameExcel, parametersExcel),
                        "AuditLog" => await _excelExportService.GenerateAuditExcelAsync(data, userNameExcel, parametersExcel),
                        "UserClaim" => await _excelExportService.GeneratePermissionsExcelAsync(data, userNameExcel, parametersExcel),
                        _ => await _excelExportService.GenerateExcelAsync(
                            execution.ReportTemplate.Name,
                            execution.ReportTemplate.Description ?? "Reporte generado automáticamente",
                            data, userNameExcel, parametersExcel)
                    };
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;

                default:
                    throw new NotSupportedException($"Formato no soportado: {format}");
            }

            await File.WriteAllBytesAsync(tempPath, content);
            return (content, tempPath);
        }

        /// <summary>
        /// Genera contenido CSV desde datos
        /// </summary>
        private byte[] GenerateCsvContent(IEnumerable<object> data)
        {
            if (!data.Any())
                return System.Text.Encoding.UTF8.GetBytes("No hay datos para exportar");

            var firstItem = data.First();
            var properties = firstItem.GetType().GetProperties();

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8);

            // Headers
            writer.WriteLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

            // Data
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item)?.ToString() ?? "";
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                });
                writer.WriteLine(string.Join(",", values));
            }

            writer.Flush();
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Obtiene el content type según el formato
        /// </summary>
        private string GetContentType(string format)
        {
            return format.ToLower() switch
            {
                "json" => "application/json",
                "csv" => "text/csv",
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
}
