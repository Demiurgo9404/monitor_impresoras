using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class CostPolicy : BaseEntity
    {
        public string PolicyName { get; set; } = string.Empty;
        public decimal CostPerPage { get; set; }
        public decimal CostPerColorPage { get; set; }
        public decimal CostPerScan { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<CostCalculationHistory>? Calculations { get; set; }
    }
}
