using System;
using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Represents a history record for cost calculations
    /// </summary>
    public class CostCalculationHistory : BaseEntity
    {
        /// <summary>
        /// ID of the printer this calculation is for
        /// </summary>
        public Guid PrinterId { get; set; }

        /// <summary>
        /// The printer this calculation is for
        /// </summary>
        public virtual Printer? Printer { get; set; }

        /// <summary>
        /// Date and time when the calculation was performed
        /// </summary>
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Total cost calculated
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Cost per page (monochrome)
        /// </summary>
        public decimal CostPerPageBW { get; set; }

        /// <summary>
        /// Cost per page (color)
        /// </summary>
        public decimal CostPerPageColor { get; set; }

        /// <summary>
        /// Total pages printed (monochrome)
        /// </summary>
        public long TotalPagesBW { get; set; }

        /// <summary>
        /// Total pages printed (color)
        /// </summary>
        public long TotalPagesColor { get; set; }

        /// <summary>
        /// Total cost of monochrome printing
        /// </summary>
        public decimal TotalCostBW { get; set; }

        /// <summary>
        /// Total cost of color printing
        /// </summary>
        public decimal TotalCostColor { get; set; }

        /// <summary>
        /// Additional costs (e.g., maintenance, supplies)
        /// </summary>
        public decimal AdditionalCosts { get; set; }

        /// <summary>
        /// Notes or details about the calculation
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID of the user who performed the calculation
        /// </summary>
        public Guid? CalculatedBy { get; set; }

        /// <summary>
        /// ID of the tenant this calculation belongs to
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// The tenant this calculation belongs to
        /// </summary>
        public virtual Tenant? Tenant { get; set; }
    }
}
