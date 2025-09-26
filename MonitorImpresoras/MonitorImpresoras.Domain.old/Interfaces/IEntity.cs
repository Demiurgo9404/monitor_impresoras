using System;

namespace MonitorImpresoras.Domain.Interfaces
{
    /// <summary>
    /// Interfaz base para todas las entidades
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Identificador único de la entidad
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Fecha de creación de la entidad
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización de la entidad
        /// </summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indica si la entidad está activa
        /// </summary>
        bool IsActive { get; set; }
    }
}
