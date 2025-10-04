using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de proyecto - QOPIQ
    /// Representa los proyectos donde se instalan impresoras
    /// </summary>
    public class Project : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        public Guid CompanyId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Completed, Cancelled

        public bool IsActive { get; set; } = true;

        // Configuración de monitoreo
        public int MonitoringIntervalMinutes { get; set; } = 5;
        public bool EnableRealTimeAlerts { get; set; } = true;
        public bool EnableAutomaticReports { get; set; } = true;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<Printer> Printers { get; set; } = new List<Printer>();
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
