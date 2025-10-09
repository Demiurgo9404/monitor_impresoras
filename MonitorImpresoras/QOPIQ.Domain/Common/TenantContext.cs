using System;

namespace QOPIQ.Domain.Common
{
    /// <summary>
    /// Contexto de tenant para operaciones multi-tenant
    /// </summary>
    public class TenantContext
    {
        /// <summary>
        /// Identificador único del tenant
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Clave única del tenant para URLs y configuración
        /// </summary>
        public string TenantKey { get; set; } = string.Empty;

        /// <summary>
        /// Connection string específica del tenant
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del tenant
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tier de suscripción del tenant
        /// </summary>
        public string Tier { get; set; } = "Free"; // Free, Professional, Enterprise

        /// <summary>
        /// Si el tenant está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de expiración del tenant (para suscripciones)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public TenantContext() { }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        public TenantContext(Guid tenantId, string tenantKey, string connectionString, string name, string tier)
        {
            TenantId = tenantId;
            TenantKey = tenantKey;
            ConnectionString = connectionString;
            Name = name;
            Tier = tier;
        }

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

