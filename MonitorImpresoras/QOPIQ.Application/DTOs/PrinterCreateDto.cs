using System;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Application.DTOs
{
    public class PrinterCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
