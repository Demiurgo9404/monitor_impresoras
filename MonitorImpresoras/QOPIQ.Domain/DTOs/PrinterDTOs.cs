using System;
using System.Collections.Generic;

namespace QOPIQ.Domain.DTOs
{
    public class PrinterDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<PrinterConsumableDTO> Consumables { get; set; } = new List<PrinterConsumableDTO>();
    }

    public class CreatePrinterDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdatePrinterDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PrinterConsumableDTO
    {
        public Guid Id { get; set; } = default;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public int? MaxCapacity { get; set; } = default;
        public int? CurrentLevel { get; set; } = default;
        public string Unit { get; set; } = string.Empty;
        public int? WarningLevel { get; set; } = default;
        public int? CriticalLevel { get; set; } = default;
        public DateTime? LastUpdated { get; set; } = default;
    }

    public class PrinterListDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? TonerLevel { get; set; }
        public int? DrumLevel { get; set; }
    }
}

