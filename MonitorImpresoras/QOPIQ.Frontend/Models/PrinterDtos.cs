using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Frontend.Models;

public class PrinterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string Status { get; set; } = "Unknown";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class PrinterCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio")]
    [StringLength(100, ErrorMessage = "El modelo no puede tener más de 100 caracteres")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección IP es obligatoria")]
    [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", 
        ErrorMessage = "Formato de IP inválido (ejemplo: 192.168.1.1)")]
    public string IpAddress { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La ubicación no puede tener más de 200 caracteres")]
    public string? Location { get; set; }
}

public class PrinterUpdateDto : PrinterCreateDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PrinterStatusDto
{
    public Guid PrinterId { get; set; }
    public string Status { get; set; } = "Unknown";
    public string? StatusMessage { get; set; }
    public DateTime LastChecked { get; set; }
    public Dictionary<string, string> Metrics { get; set; } = new();
}
