using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de rol del sistema
    /// </summary>
    public class Role : IdentityRole<string>
    {
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
