using System;

namespace MonitorImpresoras.Domain.Common
{
    /// <summary>
    /// Clase base abstracta para todas las entidades
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// Identificador único de la entidad
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha de creación de la entidad
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización de la entidad
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indica si la entidad está activa
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Actualiza la fecha de modificación
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
