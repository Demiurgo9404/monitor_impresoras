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
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Fecha de creación de la entidad
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización de la entidad
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Marca la entidad como actualizada
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
