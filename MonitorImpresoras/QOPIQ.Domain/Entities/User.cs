using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address (used for login)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// User's role in the system
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Indicates if the user is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date and time of the last login
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// User's department
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// User's job title
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User's profile image URL
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Indicates if the user's email is confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Security stamp for user's security-related operations
        /// </summary>
        public string? SecurityStamp { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Navigation property for user's print jobs
        /// </summary>
        public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
    }
}
