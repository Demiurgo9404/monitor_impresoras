using System;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para representar una impresora
    /// </summary>
    public class PrinterDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; }
        
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "La dirección IP es obligatoria")]
        [StringLength(15, ErrorMessage = "La dirección IP no es válida")]
        public string IpAddress { get; set; }
        
        [StringLength(50, ErrorMessage = "El número de serie no puede tener más de 50 caracteres")]
        public string SerialNumber { get; set; }
        
        [StringLength(50, ErrorMessage = "El modelo no puede tener más de 50 caracteres")]
        public string Model { get; set; }
        
        [StringLength(50, ErrorMessage = "La marca no puede tener más de 50 caracteres")]
        public string Brand { get; set; }
        
        [StringLength(50, ErrorMessage = "La ubicación no puede tener más de 50 caracteres")]
        public string Location { get; set; }
        
        [StringLength(50, ErrorMessage = "El departamento no puede tener más de 50 caracteres")]
        public string Department { get; set; }
        
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public int? BlackInkLevel { get; set; }
        public int? CyanInkLevel { get; set; }
        public int? MagentaInkLevel { get; set; }
        public int? YellowInkLevel { get; set; }
        public int? PageCount { get; set; }
        public DateTime? LastChecked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO para crear una nueva impresora
    /// </summary>
    public class CreatePrinterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; }
        
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "La dirección IP es obligatoria")]
        [StringLength(15, ErrorMessage = "La dirección IP no es válida")]
        public string IpAddress { get; set; }
        
        [StringLength(50, ErrorMessage = "El número de serie no puede tener más de 50 caracteres")]
        public string SerialNumber { get; set; }
        
        [StringLength(50, ErrorMessage = "El modelo no puede tener más de 50 caracteres")]
        public string Model { get; set; }
        
        [StringLength(50, ErrorMessage = "La marca no puede tener más de 50 caracteres")]
        public string Brand { get; set; }
        
        [StringLength(50, ErrorMessage = "La ubicación no puede tener más de 50 caracteres")]
        public string Location { get; set; }
        
        [StringLength(50, ErrorMessage = "El departamento no puede tener más de 50 caracteres")]
        public string Department { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una impresora existente
    /// </summary>
    public class UpdatePrinterDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Name { get; set; }
        
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "La dirección IP es obligatoria")]
        [StringLength(15, ErrorMessage = "La dirección IP no es válida")]
        public string IpAddress { get; set; }
        
        [StringLength(50, ErrorMessage = "El número de serie no puede tener más de 50 caracteres")]
        public string SerialNumber { get; set; }
        
        [StringLength(50, ErrorMessage = "El modelo no puede tener más de 50 caracteres")]
        public string Model { get; set; }
        
        [StringLength(50, ErrorMessage = "La marca no puede tener más de 50 caracteres")]
        public string Brand { get; set; }
        
        [StringLength(50, ErrorMessage = "La ubicación no puede tener más de 50 caracteres")]
        public string Location { get; set; }
        
        [StringLength(50, ErrorMessage = "El departamento no puede tener más de 50 caracteres")]
        public string Department { get; set; }
        
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO para el estado de una impresora
    /// </summary>
    public class PrinterStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
        public int? BlackInkLevel { get; set; }
        public int? CyanInkLevel { get; set; }
        public int? MagentaInkLevel { get; set; }
        public int? YellowInkLevel { get; set; }
        public int? PageCount { get; set; }
        public DateTime? LastChecked { get; set; }
    }
}
