using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    /// <summary>
    /// Entidad de usuario del sistema
    /// </summary>
    public class User : IdentityUser<string>
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Department { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string RefreshToken { get; set; } = string.Empty;

        public DateTime? RefreshTokenExpiryTime { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();

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
