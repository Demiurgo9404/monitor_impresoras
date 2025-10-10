using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    public class Alert : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public AlertType AlertType { get; set; }
        public Guid? EntityId { get; set; } // ID of the related entity (Printer, Consumable, etc.)
        public string EntityType { get; set; } = string.Empty; // Type of the related entity
        public bool IsActive { get; set; } = true;
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public Guid TenantId { get; set; }
    }
}
