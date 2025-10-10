using System;

namespace QOPIQ.Domain.Common
{
    /// <summary>
    /// Interfaz base para todas las entidades
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Identificador único de la entidad
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Fecha de creación de la entidad
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización de la entidad
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Marca la entidad como actualizada
        /// </summary>
        void MarkAsUpdated();
    }
}

