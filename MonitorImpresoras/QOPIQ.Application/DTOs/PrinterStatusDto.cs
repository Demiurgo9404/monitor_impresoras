using System;
using System.Collections.Generic;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Application.DTOs
{
    public class PrinterStatusDto
    {
        public Guid PrinterId { get; set; }
        public PrinterStatus Status { get; set; } = PrinterStatus.Unknown;
        public string TonerLevel { get; set; } = "100%";
        public int TonerLevelPercentage { get; set; } = 100;
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();
        public string StatusMessage { get; set; } = string.Empty;
    }
}
