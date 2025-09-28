using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs.Printers
{
    /// <summary>
    /// DTO para representar impresoras en respuestas de la API
    /// </summary>
    public class PrinterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public int? PageCount { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO para crear nuevas impresoras
    /// </summary>
    public class CreatePrinterDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty!;

        [Required(ErrorMessage = "El modelo es requerido")]
        [MaxLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres")]
        public string Model { get; set; } = string.Empty!;

        [Required(ErrorMessage = "El número de serie es requerido")]
        [MaxLength(100, ErrorMessage = "El número de serie no puede exceder los 100 caracteres")]
        public string SerialNumber { get; set; } = string.Empty!;

        [Required(ErrorMessage = "La dirección IP es requerida")]
        [MaxLength(50, ErrorMessage = "La dirección IP no puede exceder los 50 caracteres")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Debe ser una dirección IP válida")]
        public string IpAddress { get; set; } = string.Empty!;

        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres")]
        public string Location { get; set; } = string.Empty!;

        [MaxLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string Status { get; set; } = "Unknown";

        [MaxLength(50, ErrorMessage = "La cadena de comunidad no puede exceder los 50 caracteres")]
        public string CommunityString { get; set; } = "public";

        public int? SnmpPort { get; set; } = 161;

        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string Notes { get; set; } = string.Empty!;
    }

    /// <summary>
    /// DTO para actualizar impresoras existentes
    /// </summary>
    public class UpdatePrinterDto
    {
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? Name { get; set; }

        [MaxLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres")]
        public string? Model { get; set; }

        [MaxLength(100, ErrorMessage = "El número de serie no puede exceder los 100 caracteres")]
        public string? SerialNumber { get; set; }

        [MaxLength(50, ErrorMessage = "La dirección IP no puede exceder los 50 caracteres")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Debe ser una dirección IP válida")]
        public string? IpAddress { get; set; }

        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres")]
        public string? Location { get; set; }

        [MaxLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string? Status { get; set; }

        public bool? IsOnline { get; set; }

        [MaxLength(50, ErrorMessage = "La cadena de comunidad no puede exceder los 50 caracteres")]
        public string? CommunityString { get; set; }

        public int? SnmpPort { get; set; }

        public int? PageCount { get; set; }

        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string? Notes { get; set; }

        [MaxLength(1000, ErrorMessage = "El último error no puede exceder los 1000 caracteres")]
        public string? LastError { get; set; }
    }
}
