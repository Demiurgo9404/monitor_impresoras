using System;
using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a printer consumable (toner, drum, etc.)
    /// </summary>
    public class Consumable : BaseEntity
    {
        /// <summary>
        /// Type of consumable (toner, drum, waste container, etc.)
        /// </summary>
        public QOPIQ.Domain.Enums.ConsumableType Type { get; set; }

        /// <summary>
        /// Current level of the consumable (percentage)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Indicates if the consumable is low (below threshold)
        /// </summary>
        public bool IsLow { get; set; }

        /// <summary>
        /// Indicates if the consumable is empty
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Color of the consumable (for color printers)
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Model/Part number of the consumable
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Date when the consumable was installed
        /// </summary>
        public DateTime? InstalledDate { get; set; }

        /// <summary>
        /// Expected lifespan (in pages)
        /// </summary>
        public int? ExpectedYield { get; set; }

        /// <summary>
        /// Current page count since installation
        /// </summary>
        public int? CurrentPageCount { get; set; }

        /// <summary>
        /// Foreign key to the associated printer
        /// </summary>
        public Guid PrinterId { get; set; }

        /// <summary>
        /// Navigation property to the associated printer
        /// </summary>
        public virtual Printer? Printer { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
