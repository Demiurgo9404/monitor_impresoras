using QOPIQ.Domain.Entities;
using System;
using System.Collections.Generic;

namespace QOPIQ.Application.DTOs
{
    public class ReportDataDto
    {
        public string TenantId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Printer> Printers { get; set; } = new();
        public List<PrintJob> PrintJobs { get; set; } = new();
        public List<Alert> Alerts { get; set; } = new();
        public List<PrinterConsumable> Consumables { get; set; } = new();
        public ReportSummary Summary { get; set; } = new();
    }

    public class ReportSummary
    {
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int OfflinePrinters { get; set; }
        public int TotalPrintJobs { get; set; }
        public int TotalPages { get; set; }
        public int ColorPages { get; set; }
        public int BlackAndWhitePages { get; set; }
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public decimal TotalCost { get; set; }
        public decimal ColorCost { get; set; }
        public decimal BlackAndWhiteCost { get; set; }
    }
}
