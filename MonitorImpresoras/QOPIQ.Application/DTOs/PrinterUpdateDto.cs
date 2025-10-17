using System;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Application.DTOs
{
    public class PrinterUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? Location { get; set; }
        public PrinterStatus Status { get; set; } = PrinterStatus.Offline;
        public string? StatusMessage { get; set; }
        public bool IsOnline { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
