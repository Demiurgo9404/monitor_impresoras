using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Entidad de usuario del sistema - QOPIQ Multi-Tenant
    /// </summary>
    public class User : IdentityUser<string>
    {
        // Multi-tenant support
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;

        public Guid? CompanyId { get; set; }
        
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Department { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "User"; // SuperAdmin, CompanyAdmin, ProjectManager, User, Viewer

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string RefreshToken { get; set; } = string.Empty;

        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        // Propiedades de auditoría como BaseEntity
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Navigation properties
        public virtual Company? Company { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

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
        
        /// <summary>
        /// Marca la entidad como actualizada
        /// </summary>
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

