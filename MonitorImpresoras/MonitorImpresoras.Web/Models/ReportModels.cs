namespace MonitorImpresoras.Web.Models
{
    public class ReportSummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string FileFormat { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public long FileSizeBytes { get; set; }
        public bool EmailSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public int TotalPrinters { get; set; }
        public int TotalPrintsBW { get; set; }
        public int TotalPrintsColor { get; set; }
        public decimal TotalCostBW { get; set; }
        public decimal TotalCostColor { get; set; }
    }

    public class ReportListResponse
    {
        public List<ReportSummary> Reports { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class GenerateReportRequest
    {
        public Guid ProjectId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Format { get; set; } = "PDF";
        public string Title { get; set; } = string.Empty;
        public bool IncludeCounters { get; set; } = true;
        public bool IncludeConsumables { get; set; } = true;
        public bool IncludeCosts { get; set; } = true;
        public bool IncludeCharts { get; set; } = true;
        public bool SendByEmail { get; set; } = false;
        public string[]? EmailRecipients { get; set; }
    }

    public class ScheduledReportSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public string EmailRecipients { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateScheduledReportRequest
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public string Format { get; set; } = "PDF";
        public string[] EmailRecipients { get; set; } = Array.Empty<string>();
        public bool IncludeCounters { get; set; } = true;
        public bool IncludeConsumables { get; set; } = true;
        public bool IncludeCosts { get; set; } = true;
        public bool IncludeCharts { get; set; } = true;
    }

    public class ReportTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string PreviewImageUrl { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string RecommendedFor { get; set; } = string.Empty;
    }

    public class CalendarEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string Color { get; set; } = "#2563eb";
        public bool AllDay { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public object? ExtendedProps { get; set; }
    }

    public class ReportStats
    {
        public int TotalReports { get; set; }
        public Dictionary<string, int> ReportsByType { get; set; } = new();
        public Dictionary<string, int> ReportsByStatus { get; set; } = new();
        public Dictionary<string, int> ReportsByFormat { get; set; } = new();
        public List<ReportSummary> RecentReports { get; set; } = new();
    }

    public class ScheduledReportStats
    {
        public int TotalScheduledReports { get; set; }
        public int ActiveReports { get; set; }
        public int InactiveReports { get; set; }
        public Dictionary<string, int> ReportsByType { get; set; } = new();
        public Dictionary<string, int> ReportsByFormat { get; set; } = new();
        public List<ScheduledReportSummary> NextExecutions { get; set; } = new();
        public List<ScheduledReportSummary> RecentExecutions { get; set; } = new();
    }
}
