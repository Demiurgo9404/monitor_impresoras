using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.Common
{
    /// <summary>
    /// Entidad para gestión de tenants en el sistema multi-tenant
    /// </summary>
    public class Tenant : BaseEntity
    {
        /// <summary>
        /// Clave única del tenant para URLs y configuración
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TenantKey { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del tenant
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tier de suscripción del tenant
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Tier { get; set; } = "Free"; // Free, Basic, Professional, Enterprise

        /// <summary>
        /// Connection string específica del tenant
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el tenant está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Máximo número de impresoras permitidas
        /// </summary>
        public int MaxPrinters { get; set; } = 5; // Free tier default

        /// <summary>
        /// Máximo número de usuarios permitidos
        /// </summary>
        public int MaxUsers { get; set; } = 10; // Free tier default

        /// <summary>
        /// Fecha de expiración del tenant (para suscripciones)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Verifica si el tenant está expirado
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

        /// <summary>
        /// Obtiene información resumida del tenant
        /// </summary>
        public override string ToString()
        {
            return $"Tenant: {Name} ({TenantKey}) - Tier: {Tier} - Active: {IsActive}";
        }
    }
}

