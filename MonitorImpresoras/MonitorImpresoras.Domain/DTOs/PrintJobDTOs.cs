using System;
using System.Collections.Generic;

namespace MonitorImpresoras.Domain.DTOs
{
    public class PrintJobDTO
    {
        public Guid Id { get; set; }
        public Guid PrinterId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public int Pages { get; set; }
        public bool IsColor { get; set; }
        public bool IsDuplex { get; set; }
        public DateTime PrintedAt { get; set; }
        public string JobStatus { get; set; } = string.Empty;
        public decimal? Cost { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class CreatePrintJobDTO
    {
        public Guid PrinterId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public int Pages { get; set; }
        public bool IsColor { get; set; }
        public bool IsDuplex { get; set; }
    }

    public class PrintJobFilterDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PrinterId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool? IsColor { get; set; }
        public string JobStatus { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PrintJobSummaryDTO
    {
        public int TotalJobs { get; set; }
        public int TotalPages { get; set; }
        public int ColorPages { get; set; }
        public int BlackAndWhitePages { get; set; }
        public decimal TotalCost { get; set; }
        public Dictionary<string, int> JobsByPrinter { get; set; } = new();
        public Dictionary<string, int> PagesByUser { get; set; } = new();
    }
}
