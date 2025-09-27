using System.ComponentModel.DataAnnotations;
using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un departamento en el sistema
    /// </summary>
    public class Department : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Guid TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
