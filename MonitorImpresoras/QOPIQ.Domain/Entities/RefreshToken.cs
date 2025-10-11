using System;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a refresh token used for JWT token refresh functionality
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// The refresh token value
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Expiration date and time of the refresh token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Date and time when the token was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Indicates if the token has been revoked
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Date and time when the token was revoked (if applicable)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// The IP address that created the token
        /// </summary>
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// The IP address that revoked the token (if applicable)
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// The reason for revocation (if applicable)
        /// </summary>
        public string? ReasonRevoked { get; set; }

        /// <summary>
        /// The ID of the user who owns this refresh token
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the user who owns this refresh token
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Checks if the refresh token is expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Checks if the refresh token is active (not expired and not revoked)
        /// </summary>
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
