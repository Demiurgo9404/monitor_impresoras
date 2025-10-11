using System;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Relación entre Usuarios y Tenants (Inquilinos) para soporte multi-tenancy.
    /// </summary>
    public class TenantUser : BaseAuditableEntity
    {
        /// <summary>
        /// ID del inquilino (tenant).
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// ID del usuario.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Fecha en que el usuario fue agregado al tenant.
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si el usuario es administrador del tenant.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Indica si el usuario está activo en el tenant.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha hasta la cual el usuario tiene acceso al tenant (null = acceso indefinido).
        /// </summary>
        public DateTime? AccessUntil { get; set; }

        /// <summary>
        /// Inquilino (tenant) asociado.
        /// </summary>
        public virtual Tenant Tenant { get; set; } = null!;

        /// <summary>
        /// Usuario asociado.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}
