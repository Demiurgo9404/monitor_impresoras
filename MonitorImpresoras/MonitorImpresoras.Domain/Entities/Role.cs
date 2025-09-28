using Microsoft.AspNetCore.Identity;

namespace MonitorImpresoras.Domain.Entities
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navegación
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }
}
