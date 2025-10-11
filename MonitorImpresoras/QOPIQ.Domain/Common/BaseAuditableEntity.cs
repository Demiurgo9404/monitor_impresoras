using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QOPIQ.Domain.Common
{
    /// <summary>
    /// Clase base para entidades que requieren seguimiento de auditoría.
    /// Proporciona propiedades estándar para el seguimiento de creación y modificación.
    /// </summary>
    public abstract class BaseAuditableEntity : BaseEntity
    {
        /// <summary>
        /// Fecha y hora en que se creó la entidad (UTC).
        /// Se establece automáticamente al momento de la creación.
        /// </summary>
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identificador del usuario que creó la entidad.
        /// Puede ser nulo si el sistema realizó la creación.
        /// </summary>
        [MaxLength(450)] // Tamaño estándar para IDs de usuario de Identity
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Fecha y hora de la última modificación de la entidad (UTC).
        /// Es nulo si la entidad nunca ha sido modificada.
        /// </summary>
        [Column(TypeName = "timestamp with time zone")]
        public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identificador del usuario que realizó la última modificación.
        /// Es nulo si la entidad nunca ha sido modificada o si el sistema realizó la modificación.
        /// </summary>
        [MaxLength(450)] // Tamaño estándar para IDs de usuario de Identity
        public string? UpdatedBy { get; set; }
        public DateTime? Deleted { get; set; }

        /// <summary>
        /// Identificador del usuario que eliminó la entidad.
        /// Es nulo si la entidad no ha sido eliminada o si el sistema realizó la eliminación.
        /// </summary>
        [MaxLength(450)]
        public string? DeletedBy { get; set; }
    }
}
