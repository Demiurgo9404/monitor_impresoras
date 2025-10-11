using System;
using System.Collections.Generic;

namespace QOPIQ.Application.DTOs
{
    public class PrinterStatusDto
    {
        public Guid PrinterId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public bool IsOnline { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();
    }
}
