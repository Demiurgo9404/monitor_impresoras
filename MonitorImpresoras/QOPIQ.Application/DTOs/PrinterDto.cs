using System;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Application.DTOs
{
    public class PrinterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? Location { get; set; }
        public PrinterStatus Status { get; set; } = PrinterStatus.Offline;
        public string? StatusMessage { get; set; }
        public DateTime? LastStatusUpdate { get; set; }
        public bool IsOnline { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
