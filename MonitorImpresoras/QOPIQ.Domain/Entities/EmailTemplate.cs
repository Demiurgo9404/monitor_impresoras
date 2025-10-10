using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents an email template used for sending notifications
    /// </summary>
    public class EmailTemplate : BaseEntity
    {
        /// <summary>
        /// Template name (e.g., "PrinterAlert", "WelcomeEmail")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Template subject
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Template body (can include placeholders)
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if this is the default template for its type
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Template type (e.g., "Alert", "Notification", "Report")
        /// </summary>
        public string TemplateType { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the template's purpose
        /// </summary>
        public string? Description { get; set; }
    }
}
