using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad para manejar refresh tokens de forma segura en la base de datos
    /// </summary>
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        /// <summary>
        /// Identificador único del refresh token
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// El token de refresco en sí (almacenado como string base64)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Token { get; set; } = default!;

        /// <summary>
        /// ID del usuario propietario del token
        /// </summary>
        [Required]
        [MaxLength(450)] // ASP.NET Identity User ID length
        public string UserId { get; set; } = default!;

        /// <summary>
        /// Fecha y hora de expiración del token (UTC)
        /// </summary>
        [Required]
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Fecha y hora de creación del token (UTC)
        /// </summary>
        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Dirección IP desde la que se creó el token (para auditoría)
        /// </summary>
        [MaxLength(45)] // IPv6 max length
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// Indica si el token ha sido revocado
        /// </summary>
        public bool Revoked { get; set; } = false;

        /// <summary>
        /// Fecha y hora de revocación (UTC)
        /// </summary>
        public DateTime? RevokedAtUtc { get; set; }

        /// <summary>
        /// Dirección IP desde la que se revocó el token
        /// </summary>
        [MaxLength(45)]
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Token que reemplazó a este (para cadena de revocación)
        /// </summary>
        [MaxLength(100)]
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Indica si el token está activo (no revocado y no expirado)
        /// </summary>
        [NotMapped]
        public bool IsActive => !Revoked && DateTime.UtcNow < ExpiresAtUtc;

        /// <summary>
        /// Indica si el token ha expirado
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

        /// <summary>
        /// Relación con el usuario propietario
        /// </summary>
        public virtual User User { get; set; } = default!;
    }
}
