using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a printer in the system
    /// </summary>
    public class Printer : BaseEntity
    {
        /// <summary>
        /// Printer name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Printer model
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Printer IP address
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Printer status
        /// </summary>
        public PrinterStatus Status { get; set; } = PrinterStatus.Offline;

        /// <summary>
        /// Last known status message
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Date and time of the last status update
        /// </summary>
        public DateTime? LastStatusUpdate { get; set; }

        /// <summary>
        /// Indicates if the printer is online
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Printer location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Department where the printer is located
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Printer serial number
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Printer MAC address
        /// </summary>
        public string? MacAddress { get; set; }

        /// <summary>
        /// Printer firmware version
        /// </summary>
        public string? FirmwareVersion { get; set; }

        /// <summary>
        /// Printer page count (total)
        /// </summary>
        public int? PageCount { get; set; }

        /// <summary>
        /// Printer color mode (color/monochrome)
        /// </summary>
        public bool IsColor { get; set; }

        /// <summary>
        /// Indicates if the printer is a multifunction device
        /// </summary>
        public bool IsMultifunction { get; set; }

        /// <summary>
        /// Date and time when the printer was added to the system
        /// </summary>
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time of the last maintenance
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// Next scheduled maintenance date
        /// </summary>
        public DateTime? NextMaintenanceDate { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Navigation property for consumables
        /// </summary>
        public virtual ICollection<Consumable> Consumables { get; set; } = new List<Consumable>();

        /// <summary>
        /// Navigation property for alerts
        /// </summary>
        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
