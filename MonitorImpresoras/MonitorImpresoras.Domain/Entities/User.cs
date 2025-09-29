using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Usuario extendido con propiedades de seguridad y auditoría
    /// </summary>
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(500)]
        public string? Department { get; set; }

        // Propiedades de seguridad y auditoría
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha del último login exitoso (UTC)
        /// </summary>
        public DateTime? LastLoginAtUtc { get; set; }

        /// <summary>
        /// Número de intentos de login fallidos consecutivos
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// Fecha hasta la que el usuario está bloqueado (UTC)
        /// </summary>
        public DateTime? LockedUntilUtc { get; set; }

        /// <summary>
        /// Fecha de creación de la cuenta (UTC)
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización del perfil (UTC)
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Usuario que creó esta cuenta (para auditoría)
        /// </summary>
        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Usuario que actualizó esta cuenta por última vez
        /// </summary>
        [MaxLength(450)]
        public string? UpdatedByUserId { get; set; }

        // Legacy properties (mantener compatibilidad)
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navegaciones
        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public virtual ICollection<LoginAttempt>? LoginAttempts { get; set; }
        public virtual ICollection<PrintJob>? PrintJobs { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Verifica si el usuario está bloqueado por intentos fallidos
        /// </summary>
        [NotMapped]
        public bool IsLockedOut => LockedUntilUtc.HasValue && LockedUntilUtc.Value > DateTime.UtcNow;

        /// <summary>
        /// Verifica si el refresh token es válido
        /// </summary>
        public bool IsRefreshTokenValid()
        {
            return !string.IsNullOrEmpty(RefreshToken) &&
                   RefreshTokenExpiryTime.HasValue &&
                   RefreshTokenExpiryTime.Value > DateTime.UtcNow;
        }

        /// <summary>
        /// Obtiene el nombre completo del usuario
        /// </summary>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}".Trim();
        }

        /// <summary>
        /// Incrementa el contador de intentos fallidos
        /// </summary>
        public void IncrementFailedLoginAttempts()
        {
            FailedLoginAttempts++;
        }

        /// <summary>
        /// Resetea el contador de intentos fallidos
        /// </summary>
        public void ResetFailedLoginAttempts()
        {
            FailedLoginAttempts = 0;
            LockedUntilUtc = null;
        }

        /// <summary>
        /// Bloquea el usuario por un período de tiempo
        /// </summary>
        /// <param name="lockoutMinutes">Minutos de bloqueo</param>
        public void LockOut(int lockoutMinutes = 15)
        {
            LockedUntilUtc = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        /// <summary>
        /// Registra un login exitoso
        /// </summary>
        public void RecordSuccessfulLogin()
        {
            LastLoginAtUtc = DateTime.UtcNow;
            ResetFailedLoginAttempts();
        }
    }
}
