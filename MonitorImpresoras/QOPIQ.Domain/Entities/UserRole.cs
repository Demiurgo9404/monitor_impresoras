using System;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Relación muchos a muchos entre Usuarios y Roles.
    /// </summary>
    public class UserRole : BaseAuditableEntity
    {
        /// <summary>
        /// ID del usuario.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID del rol.
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Fecha en que se asignó el rol al usuario.
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario asociado a este rol.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Rol asignado al usuario.
        /// </summary>
        public virtual Role Role { get; set; } = null!;
    }
}
