using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;
using UserRoleEntity = QOPIQ.Domain.Entities.UserRole;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User : IdentityUser<Guid>
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
        /// User's role type in the system
        /// </summary>
        public UserRoleType RoleType { get; set; } = UserRoleType.User;

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
        public new string? PhoneNumber { get; set; }

        /// <summary>
        /// User's profile image URL
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Indicates if the user's email is confirmed
        /// </summary>
        public new bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Security stamp for user's security-related operations
        /// </summary>
        public new string? SecurityStamp { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// <summary>
        /// Navigation property for user's print jobs
        /// </summary>
        public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();

        /// <summary>
        /// Navigation property for user's roles
        /// </summary>
        public virtual ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();

        /// <summary>
        /// Navigation property for user's refresh tokens
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
