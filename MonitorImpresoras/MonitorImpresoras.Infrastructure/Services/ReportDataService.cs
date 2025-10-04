using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de datos para reportes
    /// </summary>
    public class ReportDataService : IReportDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<ReportDataService> _logger;

        public ReportDataService(
            ApplicationDbContext context,
            ITenantAccessor tenantAccessor,
            ILogger<ReportDataService> logger)
        {
            _context = context;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        public async Task<ReportDataDto> GetReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd)
        {
            try
            {
                _logger.LogInformation("Generating report data for project {ProjectId} from {Start} to {End}", 
                    projectId, periodStart, periodEnd);

                var project = await _context.Projects
                    .Include(p => p.Company)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                {
                    throw new ArgumentException($"Project {projectId} not found");
                }

                var reportData = new ReportDataDto
                {
                    ProjectName = project.Name,
                    CompanyName = project.Company.Name,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    GeneratedAt = DateTime.UtcNow,
                    Summary = await GetReportSummaryAsync(projectId, periodStart, periodEnd),
                    Printers = await GetPrinterReportDataAsync(projectId, periodStart, periodEnd),
                    Consumables = await GetConsumableReportDataAsync(projectId, periodStart, periodEnd),
                    Costs = await GetCostReportDataAsync(projectId, periodStart, periodEnd),
                    Alerts = await GetAlertReportDataAsync(projectId, periodStart, periodEnd)
                };

                _logger.LogInformation("Report data generated successfully for project {ProjectId}", projectId);
                return reportData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report data for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<ReportSummaryDto> GetReportSummaryAsync(Guid projectId, DateTime periodStart, DateTime periodEnd)
        {
            try
            {
                var printers = await _context.Printers
                    .Where(p => p.ProjectId == projectId.ToString())
                    .ToListAsync();

                var totalPrinters = printers.Count;
                var activePrinters = printers.Count(p => p.IsOnline);
                var inactivePrinters = totalPrinters - activePrinters;
                var printersWithErrors = printers.Count(p => p.NeedsUserAttention);

                // Calcular totales del período (simulado)
                var totalPagesPrinted = printers.Sum(p => p.TotalPagesPrinted ?? 0);
                var totalPagesBlackWhite = printers.Sum(p => p.TotalPrintsBlack ?? 0);
                var totalPagesColor = printers.Sum(p => p.TotalPrintsColor ?? 0);
                var totalScans = printers.Sum(p => p.TotalScans ?? 0);
                var totalCopies = printers.Sum(p => p.TotalCopies ?? 0);

                // Calcular tiempo activo promedio
                var averageUptime = printers.Any() ? 
                    printers.Where(p => p.LastChecked.HasValue)
                           .Average(p => p.IsOnline ? 95.0 : 0.0) : 0.0;

                // Obtener alertas del período
                var alerts = await _context.Alerts
                    .Where(a => a.TenantId == _tenantAccessor.TenantId &&
                               a.CreatedAt >= periodStart && 
                               a.CreatedAt <= periodEnd)
                    .ToListAsync();

                return new ReportSummaryDto
                {
                    TotalPrinters = totalPrinters,
                    ActivePrinters = activePrinters,
                    InactivePrinters = inactivePrinters,
                    PrintersWithErrors = printersWithErrors,
                    TotalPagesPrinted = totalPagesPrinted,
                    TotalPagesBlackWhite = totalPagesBlackWhite,
                    TotalPagesColor = totalPagesColor,
                    TotalScans = totalScans,
                    TotalCopies = totalCopies,
                    AverageUptimePercentage = averageUptime,
                    TotalAlerts = alerts.Count,
                    CriticalAlerts = alerts.Count(a => a.Severity == "Critical")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report summary");
                throw;
            }
        }

        public async Task<List<PrinterReportDataDto>> GetPrinterReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd)
        {
            try
            {
                var printers = await _context.Printers
                    .Where(p => p.ProjectId == projectId.ToString())
                    .ToListAsync();

                var printerReportData = printers.Select(p => new PrinterReportDataDto
                {
                    Name = p.Name,
                    Model = p.Model,
                    SerialNumber = p.SerialNumber,
                    Location = p.Location,
                    Status = p.Status,
                    IsOnline = p.IsOnline,
                    PagesPrintedBW = p.TotalPrintsBlack ?? 0,
                    PagesPrintedColor = p.TotalPrintsColor ?? 0,
                    TotalScans = p.TotalScans ?? 0,
                    TotalCopies = p.TotalCopies ?? 0,
                    TonerBlackLevel = p.BlackTonerLevel,
                    TonerCyanLevel = p.CyanTonerLevel,
                    TonerMagentaLevel = p.MagentaTonerLevel,
                    TonerYellowLevel = p.YellowTonerLevel,
                    FuserLevel = p.FuserLevel,
                    UptimePercentage = p.IsOnline ? 95.0 + Random.Shared.NextDouble() * 5.0 : 0.0,
                    ErrorCount = p.NeedsUserAttention ? Random.Shared.Next(1, 5) : 0,
                    LastMaintenance = p.LastMaintenance,
                    NeedsMaintenance = p.NeedsMaintenance
                }).ToList();

                return printerReportData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating printer report data");
                throw;
            }
        }

        public async Task<List<ConsumableReportDataDto>> GetConsumableReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd)
        {
            try
            {
                var printers = await _context.Printers
                    .Where(p => p.ProjectId == projectId.ToString())
                    .ToListAsync();

                var consumables = new List<ConsumableReportDataDto>();

                foreach (var printer in printers)
                {
                    // Tóner Negro
                    if (printer.BlackTonerLevel.HasValue)
                    {
                        consumables.Add(new ConsumableReportDataDto
                        {
                            PrinterName = printer.Name,
                            ConsumableType = "Toner",
                            Color = "Black",
                            CurrentLevel = printer.BlackTonerLevel,
                            Status = GetConsumableStatus(printer.BlackTonerLevel.Value),
                            EstimatedDaysRemaining = CalculateEstimatedDays(printer.BlackTonerLevel.Value),
                            ReplacementCost = 75.00m
                        });
                    }

                    // Tóner Cian
                    if (printer.CyanTonerLevel.HasValue)
                    {
                        consumables.Add(new ConsumableReportDataDto
                        {
                            PrinterName = printer.Name,
                            ConsumableType = "Toner",
                            Color = "Cyan",
                            CurrentLevel = printer.CyanTonerLevel,
                            Status = GetConsumableStatus(printer.CyanTonerLevel.Value),
                            EstimatedDaysRemaining = CalculateEstimatedDays(printer.CyanTonerLevel.Value),
                            ReplacementCost = 85.00m
                        });
                    }

                    // Tóner Magenta
                    if (printer.MagentaTonerLevel.HasValue)
                    {
                        consumables.Add(new ConsumableReportDataDto
                        {
                            PrinterName = printer.Name,
                            ConsumableType = "Toner",
                            Color = "Magenta",
                            CurrentLevel = printer.MagentaTonerLevel,
                            Status = GetConsumableStatus(printer.MagentaTonerLevel.Value),
                            EstimatedDaysRemaining = CalculateEstimatedDays(printer.MagentaTonerLevel.Value),
                            ReplacementCost = 85.00m
                        });
                    }

                    // Tóner Amarillo
                    if (printer.YellowTonerLevel.HasValue)
                    {
                        consumables.Add(new ConsumableReportDataDto
                        {
                            PrinterName = printer.Name,
                            ConsumableType = "Toner",
                            Color = "Yellow",
                            CurrentLevel = printer.YellowTonerLevel,
                            Status = GetConsumableStatus(printer.YellowTonerLevel.Value),
                            EstimatedDaysRemaining = CalculateEstimatedDays(printer.YellowTonerLevel.Value),
                            ReplacementCost = 85.00m
                        });
                    }

                    // Fusor
                    if (printer.FuserLevel.HasValue)
                    {
                        consumables.Add(new ConsumableReportDataDto
                        {
                            PrinterName = printer.Name,
                            ConsumableType = "Fuser",
                            Color = "N/A",
                            CurrentLevel = printer.FuserLevel,
                            Status = GetConsumableStatus(printer.FuserLevel.Value),
                            EstimatedDaysRemaining = CalculateEstimatedDays(printer.FuserLevel.Value),
                            ReplacementCost = 150.00m
                        });
                    }
                }

                return consumables;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating consumable report data");
                throw;
            }
        }

        public async Task<ReportCostDataDto> GetCostReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd)
        {
            try
            {
                var printers = await _context.Printers
                    .Where(p => p.ProjectId == projectId.ToString())
                    .ToListAsync();

                // Calcular costos simulados
                var totalPagesBW = printers.Sum(p => p.TotalPrintsBlack ?? 0);
                var totalPagesColor = printers.Sum(p => p.TotalPrintsColor ?? 0);

                var costPerPageBW = 0.02m; // $0.02 por página B/N
                var costPerPageColor = 0.08m; // $0.08 por página color

                var totalCostBW = totalPagesBW * costPerPageBW;
                var totalCostColor = totalPagesColor * costPerPageColor;

                var totalMaintenanceCost = printers.Count * 25.00m; // $25 por impresora por mes
                var totalConsumableCost = printers.Count * 50.00m; // $50 promedio consumibles por mes

                // Generar costos mensuales simulados
                var monthlyCosts = new List<MonthlyCostDto>();
                var currentDate = periodStart;
                
                while (currentDate <= periodEnd)
                {
                    var monthlyPagesBW = totalPagesBW / Math.Max(1, (periodEnd - periodStart).Days / 30);
                    var monthlyPagesColor = totalPagesColor / Math.Max(1, (periodEnd - periodStart).Days / 30);

                    monthlyCosts.Add(new MonthlyCostDto
                    {
                        Year = currentDate.Year,
                        Month = currentDate.Month,
                        MonthName = currentDate.ToString("MMMM"),
                        CostBW = monthlyPagesBW * costPerPageBW,
                        CostColor = monthlyPagesColor * costPerPageColor,
                        MaintenanceCost = totalMaintenanceCost,
                        ConsumableCost = totalConsumableCost,
                        TotalCost = (monthlyPagesBW * costPerPageBW) + (monthlyPagesColor * costPerPageColor) + totalMaintenanceCost + totalConsumableCost,
                        PagesBW = monthlyPagesBW,
                        PagesColor = monthlyPagesColor
                    });

                    currentDate = currentDate.AddMonths(1);
                }

                return new ReportCostDataDto
                {
                    TotalCostBW = totalCostBW,
                    TotalCostColor = totalCostColor,
                    TotalMaintenanceCost = totalMaintenanceCost,
                    TotalConsumableCost = totalConsumableCost,
                    CostPerPageBW = costPerPageBW,
                    CostPerPageColor = costPerPageColor,
                    AverageMonthlyCost = monthlyCosts.Any() ? monthlyCosts.Average(m => m.TotalCost) : 0,
                    MonthlyCosts = monthlyCosts
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cost report data");
                throw;
            }
        }

        public async Task<List<AlertReportDataDto>> GetAlertReportDataAsync(Guid projectId, DateTime periodStart, DateTime periodEnd)
        {
            try
            {
                var alerts = await _context.Alerts
                    .Where(a => a.TenantId == _tenantAccessor.TenantId &&
                               a.CreatedAt >= periodStart && 
                               a.CreatedAt <= periodEnd)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var alertReportData = alerts.Select(a => new AlertReportDataDto
                {
                    PrinterName = a.PrinterName ?? "Sistema",
                    AlertType = a.AlertType,
                    Severity = a.Severity,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt,
                    IsResolved = a.IsResolved,
                    ResolvedAt = a.ResolvedAt
                }).ToList();

                return alertReportData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating alert report data");
                throw;
            }
        }

        private string GetConsumableStatus(int level)
        {
            return level switch
            {
                <= 10 => "Critical",
                <= 25 => "Low",
                _ => "OK"
            };
        }

        private int CalculateEstimatedDays(int level)
        {
            // Estimación simple basada en el nivel actual
            return level switch
            {
                <= 10 => Random.Shared.Next(1, 7),
                <= 25 => Random.Shared.Next(7, 21),
                <= 50 => Random.Shared.Next(21, 60),
                _ => Random.Shared.Next(60, 120)
            };
        }
    }
}
