using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Representa un rol en el sistema con permisos específicos.
    /// </summary>
    public class Role : BaseAuditableEntity
    {
        /// <summary>
        /// Nombre del rol (ej. "Admin", "User", "Manager").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol y sus permisos.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica si el rol está activo en el sistema.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Colección de usuarios que tienen este rol.
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Lista de permisos asociados al rol.
        /// </summary>
        public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// Relación entre roles y permisos.
    /// </summary>
    public class RolePermission
    {
        public Guid RoleId { get; set; }
        public string Permission { get; set; }
        public virtual Role Role { get; set; }
    }
}
