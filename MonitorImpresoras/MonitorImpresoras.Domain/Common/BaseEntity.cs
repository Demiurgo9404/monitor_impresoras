using System;

namespace MonitorImpresoras.Domain.Common
{
    /// <summary>
    /// Clase base abstracta para todas las entidades con clave primaria genérica
    /// </summary>
    /// <typeparam name="TKey">Tipo de la clave primaria (string, int, Guid, etc.)</typeparam>
    public abstract class BaseEntity<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Identificador único de la entidad
        /// </summary>
        public TKey Id { get; set; } = default!;

        /// <summary>
        /// Fecha de creación de la entidad
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización de la entidad
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que creó la entidad
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Usuario que actualizó por última vez la entidad
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Marca la entidad como actualizada
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Clase base abstracta para entidades con clave primaria de tipo Guid
    /// </summary>
    public abstract class BaseEntity : BaseEntity<Guid>
    {
        public BaseEntity()
        {
            Id = Guid.NewGuid();
        }
    }

    /// <summary>
    /// Clase base abstracta para entidades con clave primaria de tipo string
    /// </summary>
    public abstract class BaseStringEntity : BaseEntity<string>
    {
        public BaseStringEntity()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
