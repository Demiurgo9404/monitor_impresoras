using System;

namespace MonitorImpresoras.Domain.Common
{
    /// <summary>
    /// Interfaz base para todas las entidades con clave primaria genérica
    /// </summary>
    /// <typeparam name="TKey">Tipo de la clave primaria (string, int, Guid, etc.)</typeparam>
    public interface IEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Identificador único de la entidad
        /// </summary>
        TKey Id { get; set; }

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

    /// <summary>
    /// Interfaz base para entidades con clave primaria de tipo string
    /// </summary>
    public interface IEntity : IEntity<string>
    {
    }
}
