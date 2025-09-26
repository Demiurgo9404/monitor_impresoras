using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual UserEntity? User { get; set; }
        public virtual Plan? Plan { get; set; }
        public virtual ICollection<SubscriptionHistory>? History { get; set; }
    }
}
