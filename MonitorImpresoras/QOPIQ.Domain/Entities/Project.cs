using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a project in the system
    /// </summary>
    public class Project : BaseAuditableEntity
    {
        /// <summary>
        /// Name of the project
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the project
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Start date of the project
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of the project
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Status of the project
        /// </summary>
        public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

        /// <summary>
        /// ID of the tenant this project belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this project belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }

        // Audit properties are inherited from BaseAuditableEntity:
        // - CreatedAt (replaces CreatedAt)
        // - CreatedBy (replaces CreatedBy)
        // - UpdatedAt (replaces ModifiedAt)
        // - UpdatedBy (replaces ModifiedBy)
        // - Deleted
        // - DeletedBy

        /// <summary>
        /// User who last modified the project
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Navigation property for printers in this project
        /// </summary>
        public virtual ICollection<Printer> Printers { get; set; } = new List<Printer>();
    }

    /// <summary>
    /// Represents the status of a project
    /// </summary>
    public enum ProjectStatus
    {
        /// <summary>
        /// Project is in planning phase
        /// </summary>
        Planned,

        /// <summary>
        /// Project is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Project is on hold
        /// </summary>
        OnHold,

        /// <summary>
        /// Project is completed
        /// </summary>
        Completed,

        /// <summary>
        /// Project is cancelled
        /// </summary>
        Cancelled
    }
}
