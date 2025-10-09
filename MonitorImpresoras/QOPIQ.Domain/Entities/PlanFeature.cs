using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    public class PlanFeature : BaseEntity
    {
        public string FeatureName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;

        public int PlanId { get; set; }
        public virtual Plan? Plan { get; set; }
    }
}

