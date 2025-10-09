using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Entidad que relaciona usuarios con roles
    /// </summary>
    public class UserRole : IdentityUserRole<string>
    {
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}

