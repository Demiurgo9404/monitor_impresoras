using QOPIQ.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.Entities
{
    public class Company : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string ContactPerson { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;
        
        [EmailAddress]
        [MaxLength(255)]
        public string ContactEmail { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string TaxId { get; set; } = string.Empty;
        
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? StartDate { get; set; }
        
        // Límites y configuración completos
        public int MaxPrinters { get; set; } = 5;
        public int MaxUsers { get; set; } = 10;
        public int MaxPolicies { get; set; } = 5;
        public int MaxProjects { get; set; } = 3;
        public long MaxStorageMB { get; set; } = 100;
        
        // Plan de suscripción
        public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;
        
        public bool IsActive { get; set; } = true;
        
        // Navegación
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}

