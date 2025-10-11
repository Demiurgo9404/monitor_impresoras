using System;
using System.Collections.Generic;

namespace QOPIQ.Domain.Models
{
    /// <summary>
    /// DTO for creating a new printer
    /// </summary>
    public class PrinterCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing printer
    /// </summary>
    public class PrinterUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? Location { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for printer status information
    /// </summary>
    public class PrinterStatusDto
    {
        public Guid PrinterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public Dictionary<string, string> Metrics { get; set; } = new();
    }

    /// <summary>
    /// DTO for printer statistics
    /// </summary>
    public class PrinterStatsDto
    {
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int OfflinePrinters { get; set; }
        public int NeedsMaintenance { get; set; }
        public int LowOnSupplies { get; set; }
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public Dictionary<string, int> ModelDistribution { get; set; } = new();
    }
}
