using System;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para representar una impresora
    /// </summary>
    public class PrinterDto
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La dirección IP es obligatoria")]
        [StringLength(15, ErrorMessage = "La dirección IP no es válida")]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El número de serie no puede tener más de 50 caracteres")]
        public string SerialNumber { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El modelo no puede tener más de 50 caracteres")]
        public string Model { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "La marca no puede tener más de 50 caracteres")]
        public string Brand { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "La ubicación no puede tener más de 50 caracteres")]
        public string Location { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El departamento no puede tener más de 50 caracteres")]
        public string Department { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Desconocido";
        public bool IsLocalPrinter { get; set; }
        [Obsolete("Usar IsLocalPrinter en su lugar")]
        public bool IsLocal { get => IsLocalPrinter; set => IsLocalPrinter = value; }
        public bool IsNetworkPrinter { get; set; }
        public bool IsColorPrinter { get; set; } = true;
        public string CommunityString { get; set; } = "public";
        public int? SnmpPort { get; set; } = 161;
        public int? BlackInkLevel { get; set; }
        public int? CyanInkLevel { get; set; }
        public int? MagentaInkLevel { get; set; }
        public int? YellowInkLevel { get; set; }
        public int? BlackTonerLevel { get; set; }
        public int? CyanTonerLevel { get; set; }
        public int? MagentaTonerLevel { get; set; }
        public int? YellowTonerLevel { get; set; }
        public int? PageCount { get; set; }
        public int? TotalPagesPrinted { get; set; }
        public int? TotalPrintsBlack { get; set; }
        public int? TotalPrintsColor { get; set; }
        public int? TotalCopies { get; set; }
        public int? TotalScans { get; set; }
        public string LastError { get; set; } = string.Empty;
        public bool NeedsUserAttention { get; set; }
        public bool PaperJam { get; set; }
        public bool LowTonerWarning { get; set; }
        public bool LowInkWarning { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public int? MaintenanceIntervalDays { get; set; } = 90;
        public int? DaysUntilMaintenance { get; set; }
        public DateTime? LastChecked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO para crear una nueva impresora
    /// </summary>
    public class CreatePrinterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La dirección IP es obligatoria")]
        [StringLength(15, ErrorMessage = "La dirección IP no es válida")]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El número de serie no puede tener más de 50 caracteres")]
        public string SerialNumber { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El modelo no puede tener más de 50 caracteres")]
        public string Model { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "La marca no puede tener más de 50 caracteres")]
        public string Brand { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "La ubicación no puede tener más de 50 caracteres")]
        public string Location { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El departamento no puede tener más de 50 caracteres")]
        public string Department { get; set; } = string.Empty;
        
        public bool IsLocalPrinter { get; set; } = true;
        public bool IsNetworkPrinter { get; set; } = false;
        public bool IsColorPrinter { get; set; } = true;
        public string CommunityString { get; set; } = "public";
        public int? SnmpPort { get; set; } = 161;
    }

    /// <summary>
    /// DTO para actualizar una impresora existente
    /// </summary>
    public class UpdatePrinterDto
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La dirección IP es obligatoria")]
        [StringLength(15, ErrorMessage = "La dirección IP no es válida")]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El número de serie no puede tener más de 50 caracteres")]
        public string SerialNumber { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El modelo no puede tener más de 50 caracteres")]
        public string Model { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "La marca no puede tener más de 50 caracteres")]
        public string Brand { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "La ubicación no puede tener más de 50 caracteres")]
        public string Location { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El departamento no puede tener más de 50 caracteres")]
        public string Department { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        public bool IsLocalPrinter { get; set; } = true;
        public bool IsNetworkPrinter { get; set; } = false;
        public bool IsColorPrinter { get; set; } = true;
        public string CommunityString { get; set; } = "public";
        public int? SnmpPort { get; set; } = 161;
    }

    /// <summary>
    /// DTO para el estado de una impresora
    /// </summary>
    public class PrinterStatusDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? BlackInkLevel { get; set; }
        public int? CyanInkLevel { get; set; }
        public int? MagentaInkLevel { get; set; }
        public int? YellowInkLevel { get; set; }
        public int? PageCount { get; set; }
        public DateTime? LastChecked { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
