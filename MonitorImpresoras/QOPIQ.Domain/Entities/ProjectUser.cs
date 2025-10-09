using QOPIQ.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Relación muchos a muchos entre Project y User - QOPIQ
    /// </summary>
    public class ProjectUser : BaseEntity
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Viewer"; // ProjectManager, Technician, Viewer

        public bool CanManagePrinters { get; set; } = false;
        public bool CanViewReports { get; set; } = true;
        public bool CanGenerateReports { get; set; } = false;
        public bool CanManageUsers { get; set; } = false;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}

