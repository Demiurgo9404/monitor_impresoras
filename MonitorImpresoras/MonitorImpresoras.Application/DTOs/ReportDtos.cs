using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para generar un nuevo reporte
    /// </summary>
    public class GenerateReportDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Custom

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        [Required]
        [MaxLength(50)]
        public string Format { get; set; } = "PDF"; // PDF, Excel, Both

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IncludeCounters { get; set; } = true;
        public bool IncludeConsumables { get; set; } = true;
        public bool IncludeCosts { get; set; } = true;
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeDetails { get; set; } = true;

        public string[]? EmailRecipients { get; set; }
        public bool SendByEmail { get; set; } = false;
    }

    /// <summary>
    /// DTO para mostrar información de reporte generado
    /// </summary>
    public class ReportDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FileFormat { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public bool EmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public string EmailRecipients { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Estadísticas del reporte
        public int TotalPrinters { get; set; }
        public int ActivePrinters { get; set; }
        public long TotalPrintsBW { get; set; }
        public long TotalPrintsColor { get; set; }
        public long TotalScans { get; set; }
        public decimal? TotalCostBW { get; set; }
        public decimal? TotalCostColor { get; set; }
        public int PrintersLowToner { get; set; }
        public int PrintersNeedMaintenance { get; set; }
    }

    /// <summary>
    /// DTO para lista paginada de reportes
    /// </summary>
    public class ReportListDto
    {
        public List<ReportDto> Reports { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// DTO para configurar reportes automáticos
    /// </summary>
    public class ScheduledReportDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty; // Cron expression
        public string Format { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public string EmailRecipients { get; set; } = string.Empty;
        public bool IncludeCounters { get; set; }
        public bool IncludeConsumables { get; set; }
        public bool IncludeCosts { get; set; }
        public bool IncludeCharts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO para crear reporte programado
    /// </summary>
    public class CreateScheduledReportDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } = "Monthly";

        [Required]
        [MaxLength(50)]
        public string Schedule { get; set; } = "0 0 1 * *"; // Primer día de cada mes

        [Required]
        [MaxLength(50)]
        public string Format { get; set; } = "PDF";

        [Required]
        public string[] EmailRecipients { get; set; } = Array.Empty<string>();

        public bool IncludeCounters { get; set; } = true;
        public bool IncludeConsumables { get; set; } = true;
        public bool IncludeCosts { get; set; } = true;
        public bool IncludeCharts { get; set; } = true;
    }

    /// <summary>
    /// DTO para datos del reporte (contenido)
    /// </summary>
    public class ReportDataDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime GeneratedAt { get; set; }

        // Resumen general
        public ReportSummaryDto Summary { get; set; } = new();

        // Datos por impresora
        public List<PrinterReportDataDto> Printers { get; set; } = new();

        // Datos de consumibles
        public List<ConsumableReportDataDto> Consumables { get; set; } = new();

        // Datos de costos
        public ReportCostDataDto Costs { get; set; } = new();

        // Alertas y mantenimiento
        public List<AlertReportDataDto> Alerts { get; set; } = new();
    }

    /// <summary>
    /// DTO para resumen del reporte
    /// </summary>
    public class ReportSummaryDto
    {
        public int TotalPrinters { get; set; }
        public int ActivePrinters { get; set; }
        public int InactivePrinters { get; set; }
        public int PrintersWithErrors { get; set; }
        public long TotalPagesPrinted { get; set; }
        public long TotalPagesBlackWhite { get; set; }
        public long TotalPagesColor { get; set; }
        public long TotalScans { get; set; }
        public long TotalCopies { get; set; }
        public double AverageUptimePercentage { get; set; }
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
    }

    /// <summary>
    /// DTO para datos de impresora en reporte
    /// </summary>
    public class PrinterReportDataDto
    {
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public long PagesPrintedBW { get; set; }
        public long PagesPrintedColor { get; set; }
        public long TotalScans { get; set; }
        public long TotalCopies { get; set; }
        public int? TonerBlackLevel { get; set; }
        public int? TonerCyanLevel { get; set; }
        public int? TonerMagentaLevel { get; set; }
        public int? TonerYellowLevel { get; set; }
        public int? FuserLevel { get; set; }
        public double UptimePercentage { get; set; }
        public int ErrorCount { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public bool NeedsMaintenance { get; set; }
    }

    /// <summary>
    /// DTO para datos de consumibles en reporte
    /// </summary>
    public class ConsumableReportDataDto
    {
        public string PrinterName { get; set; } = string.Empty;
        public string ConsumableType { get; set; } = string.Empty; // Toner, Ink, Fuser, Drum
        public string Color { get; set; } = string.Empty; // Black, Cyan, Magenta, Yellow
        public int? CurrentLevel { get; set; }
        public string Status { get; set; } = string.Empty; // OK, Low, Critical, Empty
        public DateTime? LastReplaced { get; set; }
        public int? EstimatedDaysRemaining { get; set; }
        public decimal? ReplacementCost { get; set; }
    }

    /// <summary>
    /// DTO para datos de costos en reporte
    /// </summary>
    public class ReportCostDataDto
    {
        public decimal TotalCostBW { get; set; }
        public decimal TotalCostColor { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal TotalConsumableCost { get; set; }
        public decimal CostPerPageBW { get; set; }
        public decimal CostPerPageColor { get; set; }
        public decimal AverageMonthlyCost { get; set; }
        public List<MonthlyCostDto> MonthlyCosts { get; set; } = new();
    }

    /// <summary>
    /// DTO para costos mensuales
    /// </summary>
    public class MonthlyCostDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal CostBW { get; set; }
        public decimal CostColor { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal ConsumableCost { get; set; }
        public decimal TotalCost { get; set; }
        public long PagesBW { get; set; }
        public long PagesColor { get; set; }
    }

    /// <summary>
    /// DTO para alertas en reporte
    /// </summary>
    public class AlertReportDataDto
    {
        public string PrinterName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    /// <summary>
    /// DTO para filtros de reportes
    /// </summary>
    public class ReportFiltersDto
    {
        public Guid? ProjectId { get; set; }
        public string? ReportType { get; set; }
        public string? Status { get; set; }
        public string? Format { get; set; }
        public DateTime? GeneratedFrom { get; set; }
        public DateTime? GeneratedTo { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public bool? EmailSent { get; set; }
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Tipos de reportes disponibles
    /// </summary>
    public static class ReportTypes
    {
        public const string Daily = "Daily";
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
        public const string Quarterly = "Quarterly";
        public const string Yearly = "Yearly";
        public const string Custom = "Custom";

        public static readonly string[] All = {
            Daily, Weekly, Monthly, Quarterly, Yearly, Custom
        };
    }

    /// <summary>
    /// Formatos de reporte disponibles
    /// </summary>
    public static class ReportFormats
    {
        public const string PDF = "PDF";
        public const string Excel = "Excel";
        public const string CSV = "CSV";
        public const string Both = "Both"; // PDF + Excel

        public static readonly string[] All = {
            PDF, Excel, CSV, Both
        };
    }

    /// <summary>
    /// Estados de reporte
    /// </summary>
    public static class ReportStatus
    {
        public const string Generating = "Generating";
        public const string Generated = "Generated";
        public const string Sent = "Sent";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All = {
            Generating, Generated, Sent, Failed, Cancelled
        };
    }
}
