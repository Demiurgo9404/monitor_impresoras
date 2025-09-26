using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class Plan : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<PlanFeature>? Features { get; set; }
        public virtual ICollection<Subscription>? Subscriptions { get; set; }
    }
}
