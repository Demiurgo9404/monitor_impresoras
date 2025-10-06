using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs
{
    // DTOs básicos para compilación
    public class PrinterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
    }

    public class CreatePrinterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string IpAddress { get; set; } = string.Empty;
        
        public string Location { get; set; } = string.Empty;
    }

    public class UpdatePrinterDto
    {
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class PrinterStatusDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
