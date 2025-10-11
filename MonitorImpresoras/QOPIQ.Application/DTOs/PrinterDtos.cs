using System;
using System.Collections.Generic;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Application.DTOs
{
    public class PrinterCreateDto
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }

    public class PrinterStatusDto
    {
        public Guid PrinterId { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastChecked { get; set; }
        public string StatusMessage { get; set; }
        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();
        public string Message { get; set; }
    }

    public class PrinterStatsDto
    {
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int OfflinePrinters { get; set; }
        public int NeedsMaintenance { get; set; }
        public int LowOnSupplies { get; set; }
        public Dictionary<string, int> StatusCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ModelDistribution { get; set; } = new Dictionary<string, int>();
    }

    public class PrinterUpdateDto
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class PrinterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastChecked { get; set; }
        public string StatusMessage { get; set; }
        public bool IsOnline { get; set; }
    }
}
