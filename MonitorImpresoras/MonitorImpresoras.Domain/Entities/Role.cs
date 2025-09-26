using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de rol del sistema
    /// </summary>
    public class Role : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    /// <summary>
    /// Entidad que relaciona usuarios con roles
    /// </summary>
    public class UserRole : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
