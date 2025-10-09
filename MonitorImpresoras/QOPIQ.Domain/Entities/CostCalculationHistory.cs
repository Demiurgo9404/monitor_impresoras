using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    public class CostCalculationHistory : BaseEntity
    {
        public int CostPolicyId { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalCost { get; set; }
        public int PagesProcessed { get; set; }
        public bool IsColor { get; set; }

        public virtual CostPolicy? CostPolicy { get; set; }
    }
}

