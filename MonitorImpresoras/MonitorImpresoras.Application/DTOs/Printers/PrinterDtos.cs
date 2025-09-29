using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs.Printers
{
    /// <summary>
    /// DTO para representar impresoras en respuestas de la API
    /// </summary>
    /// <example>
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174000",
    ///   "name": "HP LaserJet Pro",
    ///   "model": "LaserJet Pro M404",
    ///   "serialNumber": "ABC123XYZ",
    ///   "ipAddress": "192.168.1.100",
    ///   "location": "Oficina Principal",
    ///   "status": "Online",
    ///   "isOnline": true,
    ///   "pageCount": 15420,
    ///   "lastSeen": "2024-12-28T15:30:00Z",
    ///   "createdAt": "2024-12-28T10:00:00Z"
    /// }
    /// </example>
    public class PrinterDto
    {
        /// <summary>
        /// Identificador único de la impresora
        /// </summary>
        /// <example>123e4567-e89b-12d3-a456-426614174000</example>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre descriptivo de la impresora
        /// </summary>
        /// <example>HP LaserJet Pro</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Modelo de la impresora
        /// </summary>
        /// <example>LaserJet Pro M404</example>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Número de serie de la impresora
        /// </summary>
        /// <example>ABC123XYZ</example>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Dirección IP de la impresora en la red
        /// </summary>
        /// <example>192.168.1.100</example>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Ubicación física de la impresora
        /// </summary>
        /// <example>Oficina Principal</example>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Estado actual de la impresora
        /// </summary>
        /// <example>Online</example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la impresora está en línea y respondiendo
        /// </summary>
        /// <example>true</example>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Contador total de páginas impresas
        /// </summary>
        /// <example>15420</example>
        public int? PageCount { get; set; }

        /// <summary>
        /// Última vez que se detectó actividad de la impresora
        /// </summary>
        /// <example>2024-12-28T15:30:00Z</example>
        public DateTime? LastSeen { get; set; }

        /// <summary>
        /// Fecha de creación del registro de la impresora
        /// </summary>
        /// <example>2024-12-28T10:00:00Z</example>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO para crear nuevas impresoras
    /// </summary>
    /// <example>
    /// {
    ///   "name": "HP LaserJet Pro",
    ///   "model": "LaserJet Pro M404",
    ///   "serialNumber": "ABC123XYZ",
    ///   "ipAddress": "192.168.1.100",
    ///   "location": "Oficina Principal",
    ///   "status": "Online",
    ///   "communityString": "public",
    ///   "snmpPort": 161,
    ///   "notes": "Impresora principal del departamento de IT"
    /// }
    /// </example>
    public class CreatePrinterDto
    {
        /// <summary>
        /// Nombre descriptivo de la impresora (requerido)
        /// </summary>
        /// <example>HP LaserJet Pro</example>
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty!;

        /// <summary>
        /// Modelo de la impresora (requerido)
        /// </summary>
        /// <example>LaserJet Pro M404</example>
        [Required(ErrorMessage = "El modelo es requerido")]
        [MaxLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres")]
        public string Model { get; set; } = string.Empty!;

        /// <summary>
        /// Número de serie único de la impresora (requerido)
        /// </summary>
        /// <example>ABC123XYZ</example>
        [Required(ErrorMessage = "El número de serie es requerido")]
        [MaxLength(100, ErrorMessage = "El número de serie no puede exceder los 100 caracteres")]
        public string SerialNumber { get; set; } = string.Empty!;

        /// <summary>
        /// Dirección IP de la impresora en la red (requerida)
        /// </summary>
        /// <example>192.168.1.100</example>
        [Required(ErrorMessage = "La dirección IP es requerida")]
        [MaxLength(50, ErrorMessage = "La dirección IP no puede exceder los 50 caracteres")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Debe ser una dirección IP válida")]
        public string IpAddress { get; set; } = string.Empty!;

        /// <summary>
        /// Ubicación física de la impresora
        /// </summary>
        /// <example>Oficina Principal</example>
        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres")]
        public string Location { get; set; } = string.Empty!;

        /// <summary>
        /// Estado inicial de la impresora
        /// </summary>
        /// <example>Unknown</example>
        [MaxLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string Status { get; set; } = "Unknown";

        /// <summary>
        /// Cadena de comunidad SNMP para monitoreo
        /// </summary>
        /// <example>public</example>
        [MaxLength(50, ErrorMessage = "La cadena de comunidad no puede exceder los 50 caracteres")]
        public string CommunityString { get; set; } = "public";

        /// <summary>
        /// Puerto SNMP para comunicación (por defecto 161)
        /// </summary>
        /// <example>161</example>
        public int? SnmpPort { get; set; } = 161;

        /// <summary>
        /// Notas adicionales sobre la impresora
        /// </summary>
        /// <example>Impresora principal del departamento de IT</example>
        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string Notes { get; set; } = string.Empty!;
    }

    /// <summary>
    /// DTO para actualizar impresoras existentes
    /// </summary>
    /// <example>
    /// {
    ///   "name": "HP LaserJet Pro M404 - Actualizada",
    ///   "location": "Oficina Principal - Piso 2",
    ///   "isOnline": true,
    ///   "pageCount": 15800,
    ///   "notes": "Actualizada con nuevos drivers"
    /// }
    /// </example>
    public class UpdatePrinterDto
    {
        /// <summary>
        /// Nombre descriptivo de la impresora
        /// </summary>
        /// <example>HP LaserJet Pro M404 - Actualizada</example>
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? Name { get; set; }

        /// <summary>
        /// Modelo de la impresora
        /// </summary>
        /// <example>LaserJet Pro M404</example>
        [MaxLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres")]
        public string? Model { get; set; }

        /// <summary>
        /// Número de serie de la impresora
        /// </summary>
        /// <example>ABC123XYZ</example>
        [MaxLength(100, ErrorMessage = "El número de serie no puede exceder los 100 caracteres")]
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Dirección IP de la impresora en la red
        /// </summary>
        /// <example>192.168.1.100</example>
        [MaxLength(50, ErrorMessage = "La dirección IP no puede exceder los 50 caracteres")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Debe ser una dirección IP válida")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Ubicación física de la impresora
        /// </summary>
        /// <example>Oficina Principal - Piso 2</example>
        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres")]
        public string? Location { get; set; }

        /// <summary>
        /// Estado actual de la impresora
        /// </summary>
        /// <example>Online</example>
        [MaxLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string? Status { get; set; }

        /// <summary>
        /// Indica si la impresora está en línea
        /// </summary>
        /// <example>true</example>
        public bool? IsOnline { get; set; }

        /// <summary>
        /// Cadena de comunidad SNMP para monitoreo
        /// </summary>
        /// <example>public</example>
        [MaxLength(50, ErrorMessage = "La cadena de comunidad no puede exceder los 50 caracteres")]
        public string? CommunityString { get; set; }

        /// <summary>
        /// Puerto SNMP para comunicación
        /// </summary>
        /// <example>161</example>
        public int? SnmpPort { get; set; }

        /// <summary>
        /// Contador actual de páginas impresas
        /// </summary>
        /// <example>15800</example>
        public int? PageCount { get; set; }

        /// <summary>
        /// Notas adicionales sobre la impresora
        /// </summary>
        /// <example>Actualizada con nuevos drivers</example>
        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string? Notes { get; set; }

        /// <summary>
        /// Último error reportado por la impresora
        /// </summary>
        /// <example>Conexión SNMP fallida - Verificar credenciales</example>
        [MaxLength(1000, ErrorMessage = "El último error no puede exceder los 1000 caracteres")]
        public string? LastError { get; set; }
    }
}
