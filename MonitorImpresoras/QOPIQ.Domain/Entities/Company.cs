using System;
using System.Collections.Generic;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a company in the system
    /// </summary>
    public class Company : BaseEntity
    {
        /// <summary>
        /// Name of the company
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the company
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Company's address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Company's phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Company's email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Company's website
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Company's tax ID or VAT number
        /// </summary>
        public string? TaxId { get; set; }

        /// <summary>
        /// ID of the tenant this company belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this company belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// Date and time when the company was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created the company
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the company was last modified
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// User who last modified the company
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Navigation property for users in this company
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Navigation property for printers in this company
        /// </summary>
        public virtual ICollection<Printer> Printers { get; set; } = new List<Printer>();
    }
}
