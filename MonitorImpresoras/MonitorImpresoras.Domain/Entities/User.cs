using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
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

        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navegaciones
        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public virtual ICollection<LoginAttempt>? LoginAttempts { get; set; }
        public virtual ICollection<PrintJob>? PrintJobs { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Obtiene el nombre completo del usuario
        /// </summary>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}".Trim();
        }

        /// <summary>
        /// Verifica si el refresh token es válido
        /// </summary>
        public bool IsRefreshTokenValid()
        {
            return !string.IsNullOrEmpty(RefreshToken) &&
                   RefreshTokenExpiryTime.HasValue &&
                   RefreshTokenExpiryTime.Value > DateTime.UtcNow;
        }
    }
}
