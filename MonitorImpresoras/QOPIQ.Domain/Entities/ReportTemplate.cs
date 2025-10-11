using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a report template in the system
    /// </summary>
    public class ReportTemplate : BaseEntity
    {
        /// <summary>
        /// Name of the report template
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the report template
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Type of the report
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Format of the report (PDF, Excel, etc.)
        /// </summary>
        public ReportFormat Format { get; set; }

        /// <summary>
        /// The template content (JSON, XML, or other format)
        /// </summary>
        public string TemplateContent { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the template is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indicates if this is the default template for its type
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Date and time when the template was created
        /// </summary>
        public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created the template
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the template was last modified
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// User who last modified the template
        /// </summary>
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// Version of the template
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
