using Microsoft.AspNetCore.Identity;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Rol extendido con propiedades de auditoría y gestión
    /// </summary>
    public class Role : IdentityRole
    {
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Nivel de permisos del rol (1-100, donde 100 es máximo)
        /// </summary>
        public int PermissionLevel { get; set; } = 1;

        /// <summary>
        /// Indica si el rol es editable por usuarios con permisos menores
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Fecha de creación del rol (UTC)
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización (UTC)
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Usuario que creó el rol (para auditoría)
        /// </summary>
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Usuario que actualizó el rol por última vez
        /// </summary>
        public string? UpdatedByUserId { get; set; }

        // Navegación
        public virtual ICollection<UserRole>? UserRoles { get; set; }

        /// <summary>
        /// Verifica si el rol puede ser editado por un usuario con cierto nivel de permisos
        /// </summary>
        /// <param name="userPermissionLevel">Nivel de permisos del usuario</param>
        /// <returns>True si puede editar</returns>
        public bool CanBeEditedBy(int userPermissionLevel)
        {
            return !IsSystemRole || userPermissionLevel >= PermissionLevel;
        }

        /// <summary>
        /// Verifica si el rol puede ser eliminado
        /// </summary>
        /// <param name="userPermissionLevel">Nivel de permisos del usuario</param>
        /// <returns>True si puede eliminar</returns>
        public bool CanBeDeletedBy(int userPermissionLevel)
        {
            return !IsSystemRole && userPermissionLevel > PermissionLevel;
        }
    }
}
