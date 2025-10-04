using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de empresa - QOPIQ Multi-Tenant
    /// Representa las empresas que rentan impresoras
    /// </summary>
    public class Company : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;

        [MaxLength(20)]
        public string TaxId { get; set; } = string.Empty; // RFC, NIT, etc.

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        [MaxLength(50)]
        public string SubscriptionPlan { get; set; } = "Basic";

        public int MaxPrinters { get; set; } = 10;
        public int MaxProjects { get; set; } = 5;
        public int MaxUsers { get; set; } = 3;

        // Navigation properties
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Printer> Printers { get; set; } = new List<Printer>();
    }
}
