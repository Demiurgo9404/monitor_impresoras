using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para manejar claims granulares de usuarios en la base de datos
    /// Permite autorización más granular que los roles tradicionales
    /// </summary>
    [Table("UserClaims")]
    public class UserClaim
    {
        /// <summary>
        /// Identificador único del claim
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario propietario del claim
        /// </summary>
        [Required]
        [MaxLength(450)] // ASP.NET Identity User ID length
        public string UserId { get; set; } = default!;

        /// <summary>
        /// Tipo de claim (ej: "printers.manage", "reports.view")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ClaimType { get; set; } = default!;

        /// <summary>
        /// Valor del claim (ej: "true", "read", "write", "admin")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ClaimValue { get; set; } = default!;

        /// <summary>
        /// Descripción del claim para UI/admin
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Categoría del claim para organización (ej: "printers", "reports", "users")
        /// </summary>
        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Indica si el claim está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de creación del claim (UTC)
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización (UTC)
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Usuario que creó el claim (para auditoría)
        /// </summary>
        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Usuario que actualizó el claim por última vez
        /// </summary>
        [MaxLength(450)]
        public string? UpdatedByUserId { get; set; }

        /// <summary>
        /// Fecha de expiración opcional del claim (UTC)
        /// </summary>
        public DateTime? ExpiresAtUtc { get; set; }

        /// <summary>
        /// Indica si el claim ha expirado
        /// </summary>
        [NotMapped]
        public bool IsExpired => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value < DateTime.UtcNow;

        /// <summary>
        /// Verifica si el claim está activo y no ha expirado
        /// </summary>
        [NotMapped]
        public bool IsValid => IsActive && !IsExpired;

        // Navegación
        public virtual User User { get; set; } = default!;

        /// <summary>
        /// Crea un claim completo para ASP.NET Identity
        /// </summary>
        /// <returns>Claim para usar en el sistema de autorización</returns>
        public System.Security.Claims.Claim ToIdentityClaim()
        {
            return new System.Security.Claims.Claim(ClaimType, ClaimValue);
        }

        /// <summary>
        /// Verifica si el claim coincide con el tipo y valor especificado
        /// </summary>
        public bool Matches(string claimType, string claimValue)
        {
            return ClaimType == claimType && ClaimValue == claimValue;
        }
    }
}
